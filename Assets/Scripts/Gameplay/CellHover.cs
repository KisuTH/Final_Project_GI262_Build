using UnityEngine;
using UnityEngine.EventSystems;

namespace ColorShiftKnight.Gameplay
{
    // Sits on one square. When the mouse enters, the tooltip shows this square's name above it.
    public class CellHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private CellTooltip _tooltip;
        private string _cellName;
        private Vector2 _tipPosition;

        public void Setup(CellTooltip tooltip, string cellName, Vector2 tipPosition)
        {
            _tooltip = tooltip;
            _cellName = cellName;
            _tipPosition = tipPosition;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (_tooltip != null)
            {
                _tooltip.Show(_cellName, _tipPosition);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (_tooltip != null)
            {
                _tooltip.Hide();
            }
        }
    }
}
