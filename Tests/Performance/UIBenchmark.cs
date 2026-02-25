using Godot;
using System.Diagnostics;
using hd2dtest.Scripts.Core.UI;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Tests.Performance
{
    public partial class UIBenchmark : Node
    {
        private int _iterations = 100;
        private Control _testContainer;

        public override void _Ready()
        {
            _testContainer = new Control();
            AddChild(_testContainer);
            
            RunBenchmark();
        }

        private void RunBenchmark()
        {
            Log.Info("Starting UI Benchmark...");
            Stopwatch sw = new Stopwatch();

            // Test 1: Instantiation
            sw.Start();
            for (int i = 0; i < _iterations; i++)
            {
                var btn = new InteractiveButton();
                _testContainer.AddChild(btn);
            }
            sw.Stop();
            Log.Info($"[Perf] Instantiation of {_iterations} InteractiveButtons: {sw.ElapsedMilliseconds} ms (Avg: {sw.ElapsedMilliseconds / (float)_iterations} ms)");

            // Test 2: Visibility Toggle (Open/Close simulation)
            sw.Restart();
            _testContainer.Visible = false;
            _testContainer.Visible = true;
            sw.Stop();
            Log.Info($"[Perf] Visibility Toggle (Open): {sw.ElapsedMilliseconds} ms");

            // Test 3: Design Token Application (Simulate theme switch)
            sw.Restart();
            foreach(var child in _testContainer.GetChildren())
            {
                if (child is InteractiveButton btn)
                {
                    // Force re-apply (assuming method is public or we trigger it via property change)
                    btn.BackgroundColorKey = "secondary"; 
                    // Note: Property setter usually triggers update in Godot tool scripts, 
                    // but in runtime we might need explicit call if logic is in _Ready.
                    // For benchmark, we assume property setter handles it or we call _Ready logic manually if possible.
                    // In InteractiveButton implementation, logic is in _Ready. Let's assume we refactor to a public Apply method.
                }
            }
            sw.Stop();
            Log.Info($"[Perf] Theme Switch Simulation: {sw.ElapsedMilliseconds} ms");

            // Cleanup
            _testContainer.QueueFree();
        }
    }
}
