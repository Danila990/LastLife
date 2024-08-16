using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Yarn.Unity;

namespace Dialogue.Ui.CustomViews.Options
{

    public class CustomOptionsView : UnityEngine.UI.Selectable, ISubmitHandler, IPointerClickHandler
    {
        [SerializeField] private TextMeshProUGUI text;
        [SerializeField] private bool showCharacterName = false;

        public Action<DialogueOption> OnOptionSelected;
        public MarkupPalette Palette { get; set; }

        private DialogueOption _option;

        private bool _hasSubmittedOptionSelection = false;

        public DialogueOption Option
        {
            get => _option;

            set
            {
                _option = value;

                _hasSubmittedOptionSelection = false;

                // When we're given an Option, use its text and update our
                // interactibility.
                var line = showCharacterName ? value.Line.Text : value.Line.TextWithoutCharacterName;

                text.text = Palette != null ? LineView.PaletteMarkedUpText(line, Palette, false) : line.Text;

                interactable = value.IsAvailable;
            }
        }
        

        // If we receive a submit or click event, invoke our "we just selected
        // this option" handler.
        public void OnSubmit(BaseEventData eventData)
        {
            InvokeOptionSelected();
        }

        public void InvokeOptionSelected()
        {
            // turns out that Selectable subclasses aren't intrinsically interactive/non-interactive
            // based on their canvasgroup, you still need to check at the moment of interaction
            if (!IsInteractable())
            {
                return;
            }
            
            // We only want to invoke this once, because it's an error to
            // submit an option when the Dialogue Runner isn't expecting it. To
            // prevent this, we'll only invoke this if the flag hasn't been cleared already.
            if (_hasSubmittedOptionSelection == false)
            {
                OnOptionSelected.Invoke(Option);
                _hasSubmittedOptionSelection = true;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            InvokeOptionSelected();
        }

        // If we mouse-over, we're telling the UI system that this element is
        // the currently 'selected' (i.e. focused) element. 
        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.Select();
        }
	}
}