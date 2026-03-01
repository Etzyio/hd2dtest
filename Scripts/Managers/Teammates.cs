using System.Collections.Generic;

namespace hd2dtest.Scripts.Managers
{
    public class Teammates
    {
        private List<Modules.Player> _teammateList = [];

        public Modules.Player Player { get; set; }

        public void Add(Modules.Player teammate)
        {
            if (!_teammateList.Contains(teammate))
            {
                _teammateList.Add(teammate);
            }
        }

        public void Remove(Modules.Player teammate)
        {
            _teammateList.Remove(teammate);
        }

        public void Set(List<Modules.Player> teammates)
        {
            _teammateList = teammates ?? [];
        }

        public List<Modules.Player> Get()
        {
            return [.. _teammateList];
        }
    }
}
