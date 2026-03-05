using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules
{
    public class Teammates
    {
        private List<Player> _teammateList = [];

        public Player Player { get; set; }

        public void Add(Player teammate)
        {
            if (!_teammateList.Contains(teammate))
            {
                _teammateList.Add(teammate);
            }
        }

        public void Remove(Player teammate)
        {
            _teammateList.Remove(teammate);
        }

        public void Set(List<Player> teammates)
        {
            _teammateList = teammates ?? [];
        }

        public List<Player> Get()
        {
            return [.. _teammateList];
        }
    }
}
