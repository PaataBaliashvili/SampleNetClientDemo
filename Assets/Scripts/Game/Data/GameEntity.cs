using Game.Views;
using UnityEngine;

namespace Game.Data
{
    public class GameEntity
    {
        public ushort Id;
        private readonly GameEntityView _view;

        public GameEntity(GameEntityView view)
        {
            _view = view;
        }

        public void SetPosition(Vector3 position)
        {
            _view.transform.position = position;
        }
    }
}