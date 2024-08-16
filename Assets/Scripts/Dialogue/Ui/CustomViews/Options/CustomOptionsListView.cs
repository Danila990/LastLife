using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;

namespace Dialogue.Ui.CustomViews.Options
{
	public class CustomOptionsListView : DialogueViewBase, ICustomDialogueView
	{
		[SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private CustomOptionsView _optionViewPrefab;
        [SerializeField] private MarkupPalette _palette;

        [SerializeField] private float _fadeTime = 0.1f;

        [SerializeField] private bool _showUnavailableOptions = false;

        [Header("Last Line Components")]
        [SerializeField] private TextMeshProUGUI _lastLineText;
        [SerializeField] private GameObject _lastLineContainer;

        [SerializeField] private TextMeshProUGUI _lastLineCharacterNameText;
        [SerializeField] private GameObject _lastLineCharacterNameContainer;
        public DialogueUiController DialogueUiController { get; set; }

        public Button BackButton;

        // A cached pool of OptionView objects so that we can reuse them
        private readonly List<CustomOptionsView> _optionViews = new List<CustomOptionsView>();

        // The method we should call when an option has been selected.
        private Action<int> _optionSelected;

        // The line we saw most recently.
        private LocalizedLine _lastSeenLine;
        private int _backIndex;

        public void Start()
        {
            _canvasGroup.alpha = 0;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            BackButton.onClick.AddListener(OnClickBackButton);
        }
        
        private void OnClickBackButton()
        {
            JumpBack();
        }
        
        public void JumpBack()
        {
            //DialogueUiController.DialogueRunner.Dialogue.SetSelectedOption(0);
            //DialogueUiController.DialogueRunner.Dialogue.Continue();
            if (_backIndex > 0)
            {
                _optionSelected?.Invoke(_backIndex);
                _backIndex = -1;
            }
            //DialogueUiController.MoveBackNode();
        }
        
        public void Reset()
        {
            _canvasGroup = GetComponentInParent<CanvasGroup>();
        }

        public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
        {
            // Don't do anything with this line except note it and
            // immediately indicate that we're finished with it. RunOptions
            // will use it to display the text of the previous line.
            _lastSeenLine = dialogueLine;
            onDialogueLineFinished();
        }
        
        public override void RunOptions(DialogueOption[] dialogueOptions, Action<int> onOptionSelected)
        {
            // If we don't already have enough option views, create more
            while (dialogueOptions.Length > _optionViews.Count)
            {
                var optionView = CreateNewOptionView();
                optionView.gameObject.SetActive(false);
            }
            foreach (var optionView in _optionViews)
            {
                optionView.gameObject.SetActive(false);
            }

            // Set up all the option views
            var optionViewsCreated = 0;
            _backIndex = -1;
            BackButton.gameObject.SetActive(false);

            for (var i = 0; i < dialogueOptions.Length; i++)
            {
                var optionView = _optionViews[i];
                var option = dialogueOptions[i];

                if (option.IsAvailable == false && _showUnavailableOptions == false)
                {
                    // Don't show this option.
                    continue;
                }
                
                if (option.Line.Text.TryGetAttributeWithName("Back", out var attribute))
                {
                    _backIndex = i;
                    BackButton.gameObject.SetActive(true);
                    continue;
                }

                optionView.gameObject.SetActive(true);

                optionView.Palette = _palette;
                optionView.Option = option;

                // The first available option is selected by default
                if (optionViewsCreated == 0)
                {
                    optionView.Select();
                }

                optionViewsCreated += 1;
            }

            // Update the last line, if one is configured
            if (_lastLineContainer != null)
            {
                if (_lastSeenLine != null)
                {
                    // if we have a last line character name container
                    // and the last line has a character then we show the nameplate
                    // otherwise we turn off the nameplate
                    var line = _lastSeenLine.Text;
                    // if (_lastLineCharacterNameContainer != null)
                    // {
                    //     if (string.IsNullOrWhiteSpace(_lastSeenLine.CharacterName))
                    //     {
                    //         _lastLineCharacterNameContainer.SetActive(false);
                    //     }
                    //     else
                    //     {
                    //         line = _lastSeenLine.TextWithoutCharacterName;
                    //         _lastLineCharacterNameContainer.SetActive(true);
                    //         _lastLineCharacterNameText.text = _lastSeenLine.CharacterName;
                    //     }
                    // }
                    line = _lastSeenLine.TextWithoutCharacterName;

                    if (_palette != null)
                    {
                        _lastLineText.text = LineView.PaletteMarkedUpText(line, _palette);
                    }
                    else
                    {
                        _lastLineText.text = line.Text;
                    }

                    _lastLineContainer.SetActive(true);
                }
                else
                {
                    _lastLineContainer.SetActive(false);
                }
            }

            // Note the delegate to call when an option is selected
            _optionSelected = onOptionSelected;

            // sometimes (not always) the TMP layout in conjunction with the
            // content size fitters doesn't update the rect transform
            // until the next frame, and you get a weird pop as it resizes
            // just forcing this to happen now instead of then
            Relayout();

            // Fade it all in
            StartCoroutine(Effects.FadeAlpha(_canvasGroup, 0, 1, _fadeTime));
            return;

            /// <summary>
            /// Creates and configures a new <see cref="OptionView"/>, and adds
            /// it to <see cref="optionViews"/>.
            /// </summary>
            CustomOptionsView CreateNewOptionView()
            {
                var optionView = Instantiate(_optionViewPrefab, transform, false);
                optionView.transform.SetAsLastSibling();

                optionView.OnOptionSelected = OptionViewWasSelected;
                _optionViews.Add(optionView);

                return optionView;
            }

            /// <summary>
            /// Called by <see cref="OptionView"/> objects.
            /// </summary>
            void OptionViewWasSelected(DialogueOption option)
            {
                StartCoroutine(OptionViewWasSelectedInternal(option));

                IEnumerator OptionViewWasSelectedInternal(DialogueOption selectedOption)
                {
                    yield return StartCoroutine(FadeAndDisableOptionViews(_canvasGroup, 1, 0, _fadeTime));
                    _optionSelected(selectedOption.DialogueOptionID);
                }
            }
        }

        /// <inheritdoc />
        /// <remarks>
        /// If options are still shown dismisses them.
        /// </remarks>
        public override void DialogueComplete()
        {   
            _lastSeenLine = null;
            _optionSelected = null;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            // do we still have any options being shown?
            if (_canvasGroup.alpha > 0)
            {
                StopAllCoroutines();


                StartCoroutine(FadeAndDisableOptionViews(_canvasGroup, _canvasGroup.alpha, 0, _fadeTime));
            }
        }

        /// <summary>
        /// Fades canvas and then disables all option views.
        /// </summary>
        private IEnumerator FadeAndDisableOptionViews(CanvasGroup canvGroup, float from, float to, float fadeTime)
        {
            yield return Effects.FadeAlpha(canvGroup, from, to, fadeTime);

            // Hide all existing option views
            foreach (var optionView in _optionViews)
            {
                optionView.gameObject.SetActive(false);
            }
        }

        public void OnEnable()
        {
            Relayout();
        }

        private void Relayout()
        {
            // Force re-layout
            var layouts = GetComponentsInChildren<LayoutGroup>();

            // Perform the first pass of re-layout. This will update the inner horizontal group's sizing, based on the text size.
            foreach (var layout in layouts)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }
            
            // Perform the second pass of re-layout. This will update the outer vertical group's positioning of the individual elements.
            foreach (var layout in layouts)
            {
                LayoutRebuilder.ForceRebuildLayoutImmediate(layout.GetComponent<RectTransform>());
            }
        }
    }
}