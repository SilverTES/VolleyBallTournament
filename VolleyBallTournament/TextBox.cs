﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Core;
using Mugen.GFX;
using System;
using TextCopy;


namespace VolleyBallTournament;

public class TextBox : Node
{
    private Game _game;

    public int Id => _id;
    private int _id;
    public string Title => _title;
    private string _title = "";
    
    private string _prevText = "";
    private string _text = "";
    private Color _colorTitle = Color.Black;
    private Color _colorText = Color.Black;
    private Color _colorCursor = Color.Black;
    private Color _colorBg = Color.Gray;

    private int _cursorPosition = 0; // Position du curseur
    private int? _selectionStart = null; // Début de la sélection (null si pas de sélection)
    //private Rectangle _bounds;
    private SpriteFont _font;
    private SpriteFont _fontTitle;
    public bool IsFocus => _isFocus;
    private bool _isFocus = false;
    private float _blinkTimer = 0f;
    private bool _cursorVisible = true;
    private const float BLINK_INTERVAL = 0.5f; // Intervalle de clignotement du curseur
    private int _maxLength = 50; // Limite de caractères (modifiable)
    private float _scrollOffset = 0f; // Décalage pour le défilement
    private bool _isDragging = false; // État du glisser-déposer

    private float _moveCooldown = 0f; // Temps restant avant le prochain déplacement
    private const float MOVE_DELAY = 0.1f; // Délai en secondes entre chaque déplacement (ajustable)
    private bool _isMoving = false; // Nouvelle variable pour suivre le déplacement
    public bool IsCursorAtEnd => _isCursorAtEnd;
    private bool _isCursorAtEnd = true;
    public bool IsCursorAtHome => _isCursorAtHome;
    private bool _isCursorAtHome = true;

    public Action<TextBox> OnChange;
    public Action<TextBox> OnFocus;

    private Point? _mouseStartPosition = null; // Nouvelle variable pour suivre la position initiale du clic

    float _extend = 0f;
    float _ticExtend = 0f;

    public TextBox(Game game, Rectangle bounds, SpriteFont font, Color bgColor, Color textColor, Color cursorColor, int maxLength = 50)
    {
        _type = UID.Get<TextBox>();

        _game = game;
        //_bounds = bounds;
        _rect = bounds;
        _font = font;
        _fontTitle = font;
        _colorBg = bgColor;
        _colorText = textColor;
        _colorCursor = cursorColor;
        _maxLength = maxLength;

        SetPosition(bounds.Location.ToVector2());

        game.Window.TextInput += Window_TextInput;

        OnFocus += (t) =>
        {

        };

    }
    private void Window_TextInput(object sender, TextInputEventArgs e)
    {
        HandleTextInput(e);
    }
    public TextBox SetId(int id)
    { 
        _id = id; 
        return this;
    }
    public TextBox SetTitle(string title, Color color, SpriteFont font)
    {
        _title = title;
        _colorTitle = color;
        _fontTitle = font;

        return this;
    }
    public TextBox SetFocus(bool isFocus)
    {
        _isFocus = isFocus;
        return this;
    }
    public string Text
    {
        get => _text;
        set
        {
            _text = value.Length > _maxLength ? value.Substring(0, _maxLength) : value;
            _cursorPosition = _text.Length;
            _selectionStart = null;
            AdjustScroll();
        }
    }

    public bool IsActive => _isFocus;

    public override Node Update(GameTime gameTime)
    {
        _ticExtend += .25f;
        _extend = (float)Math.Sin(_ticExtend) * 2f;

        UpdateRect();

        if (_moveCooldown > 0)
            _moveCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

        if (AbsRect.Contains(Static.MousePos))
        {
            if (Static.Mouse.LeftButton == ButtonState.Pressed)
            {
                if (!_mouseStartPosition.HasValue)
                {
                    _isFocus = true;
                    OnFocus(this);
                    UpdateCursorFromMouse(Static.MousePos.ToPoint()); // Déplace le curseur au clic initial
                    _mouseStartPosition = Static.MousePos.ToPoint(); // Enregistre la position initiale
                    _selectionStart = null;
                }
                else if (Static.MousePos.ToPoint() != _mouseStartPosition.Value)
                {
                    // Active le dragging seulement si la souris a bougé
                    _isDragging = true;
                    UpdateSelectionFromMouse(Static.MousePos.ToPoint());
                }
            }
            else if (Static.Mouse.LeftButton == ButtonState.Released)
            {
                _isDragging = false;
                _mouseStartPosition = null; // Réinitialise la position initiale
            }
        }
        else if (Static.Mouse.LeftButton == ButtonState.Pressed)
        {
            _isFocus = false;
            _selectionStart = null;
            _isDragging = false;
            _mouseStartPosition = null;
        }

        if (!_isFocus) return base.Update(gameTime);

        _isMoving = Static.Key.IsKeyDown(Keys.Left) || Static.Key.IsKeyDown(Keys.Right) ||
                    Static.Key.IsKeyDown(Keys.Home) || Static.Key.IsKeyDown(Keys.End);

        if (!_isMoving)
        {
            _blinkTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (_blinkTimer >= BLINK_INTERVAL)
            {
                _cursorVisible = !_cursorVisible;
                _blinkTimer = 0f;
            }
        }
        else
        {
            _cursorVisible = true;
            _blinkTimer = 0f;
        }

        _isCursorAtEnd = false;
        _isCursorAtHome = false;

        HandleKeyboardInput(Static.Key);

        _isCursorAtHome = _cursorPosition == 0;
        _isCursorAtEnd = _cursorPosition == _text.Length;

        return base.Update(gameTime);
    }

    public void HandleTextInput(TextInputEventArgs e)
    {
        if (!_isFocus) return;

        _prevText = _text;

        // Supprimer la sélection ou un caractère avec Backspace
        if (e.Key == Keys.Back && _text.Length > 0)
        {
            if (_selectionStart.HasValue && _selectionStart.Value != _cursorPosition)
            {
                DeleteSelection();
            }
            else if (_cursorPosition > 0)
            {
                _text = _text.Remove(_cursorPosition - 1, 1);
                _cursorPosition--;
            }
            _selectionStart = null;
            AdjustScroll();
        }
        // Supprimer vers la droite avec Delete
        else if (e.Key == Keys.Delete && _text.Length > 0)
        {
            if (_selectionStart.HasValue && _selectionStart.Value != _cursorPosition)
            {
                DeleteSelection();
            }
            else if (_cursorPosition < _text.Length)
            {
                _text = _text.Remove(_cursorPosition, 1);
            }
            _selectionStart = null;
            AdjustScroll();
        }
        // Ajouter un caractère Unicode si limite non atteinte
        else if (!char.IsControl(e.Character) && _text.Length < _maxLength)
        {
            if (_selectionStart.HasValue && _selectionStart.Value != _cursorPosition)
            {
                DeleteSelection();
            }
            _text = _text.Insert(_cursorPosition, e.Character.ToString());
            _cursorPosition++;
            _selectionStart = null;
            AdjustScroll();
        }

        if (OnChange != null && _prevText != _text)
            OnChange(this);
    }

    private void HandleKeyboardInput(KeyboardState keyboardState)
    {
        if (_moveCooldown > 0) return;

        if (keyboardState.IsKeyDown(Keys.Left) && _cursorPosition > 0)
        {
            if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
            {
                if (!_selectionStart.HasValue) _selectionStart = _cursorPosition;
            }
            else
            {
                _selectionStart = null;
            }
            _cursorPosition--;
            _moveCooldown = MOVE_DELAY;
            AdjustScroll();
        }
        else if (keyboardState.IsKeyDown(Keys.Right) && _cursorPosition < _text.Length)
        {
            if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
            {
                if (!_selectionStart.HasValue) _selectionStart = _cursorPosition;
            }
            else
            {
                _selectionStart = null;
            }
            _cursorPosition++;
            _moveCooldown = MOVE_DELAY;
            AdjustScroll();
        }
        else if (keyboardState.IsKeyDown(Keys.Home) && _cursorPosition > 0)
        {
            if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
            {
                if (!_selectionStart.HasValue) _selectionStart = _cursorPosition;
            }
            else
            {
                _selectionStart = null;
            }
            _cursorPosition = 0;
            _moveCooldown = MOVE_DELAY;
            AdjustScroll();
        }
        else if (keyboardState.IsKeyDown(Keys.End) && _cursorPosition < _text.Length)
        {
            if (keyboardState.IsKeyDown(Keys.LeftShift) || keyboardState.IsKeyDown(Keys.RightShift))
            {
                if (!_selectionStart.HasValue) _selectionStart = _cursorPosition;
            }
            else
            {
                _selectionStart = null;
            }
            _cursorPosition = _text.Length;
            _moveCooldown = MOVE_DELAY;
            AdjustScroll();
        }
        // Copier (Ctrl+C)
        else if (keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl))
        {
            if (keyboardState.IsKeyDown(Keys.C) && _selectionStart.HasValue && _selectionStart.Value != _cursorPosition)
            {
                int start = Math.Min(_selectionStart.Value, _cursorPosition);
                int end = Math.Max(_selectionStart.Value, _cursorPosition);
                string selectedText = _text.Substring(start, end - start);

                ClipboardService.SetText(selectedText);
                //ClipboardText = selectedText;

                _moveCooldown = MOVE_DELAY;
            }
            // Couper (Ctrl+X)
            else if (keyboardState.IsKeyDown(Keys.X) && _selectionStart.HasValue && _selectionStart.Value != _cursorPosition)
            {
                int start = Math.Min(_selectionStart.Value, _cursorPosition);
                int end = Math.Max(_selectionStart.Value, _cursorPosition);
                string selectedText = _text.Substring(start, end - start);

                ClipboardService.SetText(selectedText);
                //ClipboardText = selectedText;

                _text = _text.Remove(start, end - start);
                _cursorPosition = start;
                _selectionStart = null;
                _moveCooldown = MOVE_DELAY;
                AdjustScroll();
            }
            // Coller (Ctrl+V)
            else if (keyboardState.IsKeyDown(Keys.V))
            {
                string clipboardText = ClipboardService.GetText();
                //string clipboardText = ClipboardText;

                if (_selectionStart.HasValue && _selectionStart.Value != _cursorPosition)
                {
                    DeleteSelection();
                }
                int availableLength = _maxLength - _text.Length;
                if (clipboardText.Length > availableLength)
                {
                    clipboardText = clipboardText.Substring(0, availableLength);
                }
                if (clipboardText.Length > 0)
                {
                    _text = _text.Insert(_cursorPosition, clipboardText);
                    _cursorPosition += clipboardText.Length;
                    _selectionStart = null;
                    _moveCooldown = MOVE_DELAY;
                    AdjustScroll();
                }
            }
        }
    }

    private void UpdateCursorFromMouse(Point mousePosition)
    {
        float textWidth = _font.MeasureString(_text).X;
        float mouseXRelative = mousePosition.X - (AbsRect.X + 5) + _scrollOffset; // Position relative au texte

        if (mouseXRelative > textWidth) // Si clic à droite du texte
        {
            _cursorPosition = _text.Length;
        }
        else
        {
            int newPosition = 0;
            for (int i = 0; i <= _text.Length; i++)
            {
                float currentWidth = _font.MeasureString(_text.Substring(0, i)).X;
                if (currentWidth > mouseXRelative)
                {
                    newPosition = i;
                    break;
                }
            }
            _cursorPosition = newPosition;
        }
        AdjustScroll();
    }

    private void UpdateSelectionFromMouse(Point mousePosition)
    {
        float textWidth = _font.MeasureString(_text).X;
        float mouseXRelative = mousePosition.X - (AbsRect.X + 5) + _scrollOffset; // Position relative au texte

        if (mouseXRelative > textWidth) // Si clic à droite du texte
        {
            if (!_selectionStart.HasValue) _selectionStart = _cursorPosition;
            _cursorPosition = _text.Length;
        }
        else
        {
            int newPosition = _text.Length;
            for (int i = 0; i <= _text.Length; i++)
            {
                float currentWidth = _font.MeasureString(_text.Substring(0, i)).X;
                if (currentWidth > mouseXRelative)
                {
                    newPosition = i;
                    break;
                }
            }
            if (!_selectionStart.HasValue) _selectionStart = _cursorPosition;
            _cursorPosition = newPosition;
        }
        AdjustScroll();
    }

    private void DeleteSelection()
    {
        if (!_selectionStart.HasValue || _selectionStart.Value == _cursorPosition) return;

        int start = Math.Min(_selectionStart.Value, _cursorPosition);
        int end = Math.Max(_selectionStart.Value, _cursorPosition);
        _text = _text.Remove(start, end - start);
        _cursorPosition = start;
        _selectionStart = null;
        AdjustScroll();
    }

    private void AdjustScroll()
    {
        float textWidth = _font.MeasureString(_text).X;
        float cursorX = _font.MeasureString(_text.Substring(0, _cursorPosition)).X;
        float visibleWidth = _rect.Width - 10;

        if (textWidth <= visibleWidth)
        {
            _scrollOffset = 0;
        }
        else
        {
            if (cursorX > _scrollOffset + visibleWidth)
            {
                _scrollOffset = cursorX - visibleWidth;
            }
            else if (cursorX < _scrollOffset)
            {
                _scrollOffset = cursorX;
            }
        }

        float maxScroll = Math.Max(0, textWidth - visibleWidth);
        _scrollOffset = MathHelper.Clamp(_scrollOffset, 0, maxScroll);

        //Misc.Log($"cursorX: {cursorX}, _scrollOffset: {_scrollOffset}");
    }

    public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
    {
        if (indexLayer == (int)Layers.HUD)
        {
            // Dessiner le fond du champ
            batch.Draw(GFX._whitePixel, AbsRect, _colorBg);

            batch.Rectangle(AbsRectF.Extend(_isFocus ? _extend : 0), _isFocus ? Color.Gold : Color.Transparent, 3f);

            // Sauvegarder l'état actuel du ScissorRectangle
            Rectangle? originalScissor = batch.GraphicsDevice.ScissorRectangle;

            // Appliquer le clipping sans sortir du SpriteBatch
            batch.GraphicsDevice.ScissorRectangle = AbsRect;

            // Position avec défilement
            Vector2 textPosition = new Vector2(AbsRect.X + 5 - _scrollOffset, AbsRect.Y);

            // Dessiner la sélection si elle existe
            if (_selectionStart.HasValue && _selectionStart.Value != _cursorPosition)
            {
                int start = Math.Min(_selectionStart.Value, _cursorPosition);
                int end = Math.Max(_selectionStart.Value, _cursorPosition);
                float selectionX = AbsRect.X + 5 + _font.MeasureString(_text.Substring(0, start)).X - _scrollOffset;
                float selectionWidth = _font.MeasureString(_text.Substring(start, end - start)).X;

                batch.Draw(GFX._whitePixel,
                    new Rectangle((int)selectionX, AbsRect.Y + 5, (int)selectionWidth, (int)_font.MeasureString(" ").Y),
                    Color.LightBlue);
            }

            // Dessiner le texte
            batch.DrawString(_font, _text, textPosition, _colorText);

            // Dessiner le curseur si actif
            if (_isFocus && _cursorVisible)
            {
                float cursorAbsoluteX = _font.MeasureString(_text.Substring(0, _cursorPosition)).X;
                float cursorX = AbsRect.X + 5 + MathHelper.Clamp(cursorAbsoluteX - _scrollOffset, 0, AbsRect.Width - 10);

                batch.Draw(GFX._whitePixel,
                    new Rectangle((int)cursorX, AbsRect.Y + 5, 3, (int)_font.MeasureString(" ").Y - 5),
                    _colorCursor);
            }

            // Restaurer l'état du ScissorRectangle (optionnel, selon le contexte)
            if (originalScissor.HasValue)
                batch.GraphicsDevice.ScissorRectangle = originalScissor.Value;

            batch.CenterStringXY(_fontTitle, _title, AbsRectF.TopCenter - Vector2.UnitY * 20, _isFocus ? _colorTitle : _colorTitle *.5f);
        }

        if (indexLayer == (int)Layers.Debug)
        {
            //batch.CenterStringXY(Static.FontMini, $"{_isCursorAtHome}-{_cursorPosition}:{_text.Length}-{_isCursorAtEnd}", AbsRectF.BottomCenter, Color.GreenYellow);
        }


        return base.Draw(batch, gameTime, indexLayer);
    }
}