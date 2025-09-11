using System.Collections.Generic;
using System.Collections;

namespace ALWTTT.Generation
{
    /// <summary>
    /// Orchestrates a list of stages. Call Step() to advance one stage at a time.
    /// </summary>
    public class SectorGraphStepper
    {
        private readonly List<ISectorGenStage> _stages;
        private readonly SectorGraphContext _ctx;
        private int _index;

        public SectorGraphStepper(List<ISectorGenStage> stages, SectorGraphContext ctx)
        {
            _stages = stages;
            _ctx = ctx;
            _index = 0;
        }

        public bool IsDone => _index >= _stages.Count;
        public int CurrentIndex => _index;
        public string CurrentStageName => IsDone ? "(done)" : _stages[_index].Name;
        public SectorGraphContext Context => _ctx;

        /// <summary>Advance one stage. Returns info you can show on HUD.</summary>
        public GenStepInfo Step()
        {
            if (IsDone) return new GenStepInfo { StageName = "(done)", StepIndex = _index };

            var stage = _stages[_index];
            // If the stage exposes an enumerator with sub-steps,
            // run it to first yield then stop.
            var enumerator = stage.Execute(_ctx);
            if (enumerator != null)
            {
                // consume the whole stage now (simple)
                // OR consume one yield for finer granularity.
                while (enumerator.MoveNext()) 
                { 
                    /* sub-steps could be emitted here later */ 
                }
            }

            var info = new GenStepInfo 
            { 
                StageName = stage.Name, 
                Detail = "", 
                StepIndex = _index 
            };

            _index++;
            return info;
        }

        /// <summary>Run all remaining stages immediately.</summary>
        public void RunToEnd()
        {
            while (!IsDone) Step();
        }
    }
}
