using Game.Views;
using UnityEngine;

namespace Game.Data
{
    public class Minion : GameEntity
    {
        private readonly MinionView _minionView;

        public Minion(MinionView view) : base(view)
        {
            _minionView = view;
        }
    }
}