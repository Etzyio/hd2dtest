/*
 * File: SkillExecutor.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 技能执行器，负责执行技能的各个阶段和事件。
 * 支持技能的分阶段执行、事件调度、中断处理和状态管理。
 * 
 * Key Features:
 * - 分阶段执行：支持技能的多个阶段顺序执行
 * - 事件调度：按时间顺序执行技能事件
 * - 技能中断：支持技能执行过程中的中断
 * - 状态管理：跟踪当前执行状态和阶段
 * - 信号通知：技能开始、阶段开始、技能完成、技能中断信号
 * - 完整的异常处理和日志记录
 */

using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.SkillSystem
{
    /// <summary>
    /// 技能执行器，负责执行技能的各个阶段和事件
    /// </summary>
    /// <remarks>
    /// 支持技能的分阶段执行、事件调度和中断处理
    /// </remarks>
    public partial class SkillExecutor : Node
    {
        [Signal] public delegate void SkillStartedEventHandler(string skillId);
        [Signal] public delegate void SkillPhaseStartedEventHandler(string phaseName);
        [Signal] public delegate void SkillFinishedEventHandler(string skillId);
        [Signal] public delegate void SkillInterruptedEventHandler(string skillId);

        private SkillExecutionContext _currentContext;
        private bool _isExecuting = false;
        private int _currentPhaseIndex = -1;
        private float _phaseTimer = 0f;
        private List<SkillEvent> _pendingEvents = new List<SkillEvent>();

        public void ExecuteSkill(SkillExecutionContext context)
        {
            if (_isExecuting)
            {
                Log.Warning("SkillExecutor is already executing a skill.");
                return;
            }

            if (context?.Skill == null)
            {
                Log.Error("Invalid SkillContext.");
                return;
            }

            _currentContext = context;
            _isExecuting = true;
            _currentPhaseIndex = -1;

            Log.Info($"Starting Skill: {context.Skill.SkillName}");
            EmitSignal(SignalName.SkillStarted, context.Skill.SkillId);

            NextPhase();
        }

        public void Interrupt()
        {
            if (!_isExecuting) return;

            Log.Info("Skill Interrupted.");
            _isExecuting = false;
            _pendingEvents.Clear();
            EmitSignal(SignalName.SkillInterrupted, _currentContext.Skill.SkillId);
            _currentContext = null;
        }

        private void NextPhase()
        {
            _currentPhaseIndex++;
            if (_currentPhaseIndex >= _currentContext.Skill.Phases.Count)
            {
                FinishSkill();
                return;
            }

            SkillPhase phase = _currentContext.Skill.Phases[_currentPhaseIndex];
            _phaseTimer = 0f;

            // 为该阶段准备事件
            _pendingEvents.Clear();
            if (phase.Events != null)
            {
                _pendingEvents.AddRange(phase.Events);
                // 按时间排序以防万一
                _pendingEvents.Sort((a, b) => a.NormalizedTime.CompareTo(b.NormalizedTime));
            }

            Log.Info($"Starting Phase: {phase.PhaseName} (Duration: {phase.Duration}s)");
            EmitSignal(SignalName.SkillPhaseStarted, phase.PhaseName);

            // 如果持续时间为 0，立即执行并进入下一阶段
            if (phase.Duration <= 0)
            {
                ExecuteAllPendingEvents();
                NextPhase();
            }
        }

        public override void _Process(double delta)
        {
            if (!_isExecuting || _currentContext == null) return;
            if (_currentPhaseIndex < 0 || _currentPhaseIndex >= _currentContext.Skill.Phases.Count) return;

            SkillPhase phase = _currentContext.Skill.Phases[_currentPhaseIndex];
            float dt = (float)delta;
            _phaseTimer += dt;

            // 检查事件
            // 标准化时间：0 到 1
            float normalizedTime = (phase.Duration > 0) ? (_phaseTimer / phase.Duration) : 1.0f;

            // 按顺序执行所有到期事件
            while (_pendingEvents.Count > 0)
            {
                SkillEvent evt = _pendingEvents[0];
                if (normalizedTime >= evt.NormalizedTime)
                {
                    evt.Execute(_currentContext);
                    _pendingEvents.RemoveAt(0);
                }
                else
                {
                    break; // 下一个事件还未到期
                }
            }

            // 检查阶段结束
            if (_phaseTimer >= phase.Duration)
            {
                ExecuteAllPendingEvents(); // 确保所有剩余事件（例如恰好在 1.0 时的事件）都被执行
                NextPhase();
            }
        }

        private void ExecuteAllPendingEvents()
        {
            foreach (var evt in _pendingEvents)
            {
                evt.Execute(_currentContext);
            }
            _pendingEvents.Clear();
        }

        private void FinishSkill()
        {
            Log.Info("Skill Finished.");
            _isExecuting = false;
            EmitSignal(SignalName.SkillFinished, _currentContext.Skill.SkillId);
            _currentContext = null;
        }
    }
}
