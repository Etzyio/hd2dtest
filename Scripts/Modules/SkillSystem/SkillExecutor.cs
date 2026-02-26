using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.SkillSystem
{
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
            
            // Prepare events for this phase
            _pendingEvents.Clear();
            if (phase.Events != null)
            {
                _pendingEvents.AddRange(phase.Events);
                // Sort by time just in case
                _pendingEvents.Sort((a, b) => a.NormalizedTime.CompareTo(b.NormalizedTime));
            }

            Log.Info($"Starting Phase: {phase.PhaseName} (Duration: {phase.Duration}s)");
            EmitSignal(SignalName.SkillPhaseStarted, phase.PhaseName);
            
            // If duration is 0, execute immediately and go next
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

            // Check events
            // Normalized time: 0 to 1
            float normalizedTime = (phase.Duration > 0) ? (_phaseTimer / phase.Duration) : 1.0f;
            
            // Execute all due events in order
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
                    break; // Next event is not due yet
                }
            }

            // Check phase end
            if (_phaseTimer >= phase.Duration)
            {
                ExecuteAllPendingEvents(); // Ensure anything remaining (e.g. at exactly 1.0) is executed
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
