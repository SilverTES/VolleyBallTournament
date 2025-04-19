using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Mugen.Animation;
using Mugen.Core;
using Mugen.GFX;
using Mugen.Input;
using System.IO;
using System;

namespace VolleyBallTournament
{
    public enum Phases
    {
        None,
        Pool1,
        Pool2,
        DemiFinal,
        Final,
    }

    public class ScreenPlay : Node  
    {
        private Node _prevScreenNode;
        public Node CurrentScreenNode;

        public Phases Phase;
        public PhaseRegister PhaseRegister => _phaseRegister;
        private PhaseRegister _phaseRegister;
        public PhasePool PhasePool1 => _phasePool1;
        private PhasePool _phasePool1;
        public PhasePool PhasePool2 => _phasePool2;
        private PhasePool _phasePool2;
        public PhaseDemiFinal PhaseDemiFinal => _phaseDemiFinal;
        private PhaseDemiFinal _phaseDemiFinal;

        private RotationManager _rotationManagerPhasePool;

        KeyboardState _key;

        Animate _animate;

        float _cameraX;

        Vector2 _versionPos;

        private int _nbScreen = 3;

        Vector2 _scrolling = new Vector2(0, Screen.Height - 20);

        SpriteFont _fontScrolling;
        string _textScrolling = "               -- Bienvenue au Tournoi de VolleyBall de Saint Maurice L'Exil --                 Match : Victoire = 3p, Nul = 1p, Défaite = 0p  + Bonus : écart de point et nombre de points total marqués        ";
        float _sizeTextScrolling;
        Color _colorTextScrolling;

        public ScreenPlay(Game game)
        {
            _colorTextScrolling = Color.WhiteSmoke;
            _fontScrolling = Static.FontMini;
            _sizeTextScrolling = _fontScrolling.MeasureString(_textScrolling).X;

            //var files = GetFilesInDirectory(Directory.GetCurrentDirectory(), "*.xml");
            //for (int i = 0; i < files.Length; i++)
            //{
            //    var file = files[i];
            //    Misc.Log($"-> {file} ");
            //}
            //var configFile = files[0];
            var configPhasePool = "SetupPool.xml";

            SetSize(Screen.Width, Screen.Height);

            _phaseRegister = new PhaseRegister(game, configPhasePool).SetX(Screen.Width * 0f).AppendTo(this).This<PhaseRegister>();

            _rotationManagerPhasePool = new RotationManager();
            _rotationManagerPhasePool.LoadFile(configPhasePool, _phaseRegister.GetTeams(), _phaseRegister.GetMatchs());

            _phasePool1 = new PhasePool(game, 0, "Phase de poule Brassage", _rotationManagerPhasePool, _phaseRegister).SetX(Screen.Width * 1f).AppendTo(this).This<PhasePool>();
            _phasePool2 = new PhasePool(game, 1, "Phase de poule Qualification", _rotationManagerPhasePool).SetX(Screen.Width * 2f).AppendTo(this).This<PhasePool>();

            _phasePool1.SetRotation(0, _rotationManagerPhasePool);


            _phaseDemiFinal = new PhaseDemiFinal(game, "Phase Demi Finales").SetX(Screen.Width * 3f).AppendTo(this).This<PhaseDemiFinal>();

            var matchConfigs = MatchConfig.CreateMatchConfigsDemiFinal(_phaseDemiFinal.GetTeams(), 1, 3);

            _phaseDemiFinal.GetMatch(0).SetMatchConfigs(matchConfigs);
            _phaseDemiFinal.GetMatch(1).SetMatchConfigs(matchConfigs);
            _phaseDemiFinal.GetMatch(2).SetMatchConfigs(matchConfigs);

            _animate = new Animate();
            _animate.Add("SlideLeft");
            _animate.Add("SlideRight");

            _versionPos = AbsRectF.TopRight - Vector2.UnitX * 8 + Vector2.UnitY * 16;

            //Debug
            SetPosition(_cameraX = -Screen.Width * 1, 0);


            _phasePool1.OnFinishPhase += (phasePool) =>
            {
                _phasePool2.Import16TeamsBrassageToQualification(configPhasePool, phasePool);
                Misc.Log("Finish Phase Pool Brassage");
            };
        }
        public static string[] GetFilesInDirectory(string directoryPath, string filter)
        {
            try
            {
                if (Directory.Exists(directoryPath))
                {
                    return Directory.GetFiles(directoryPath, filter); // Filtre personnalisable
                }
                return new string[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur : {ex.Message}");
                return new string[0];
            }
        }

        public override Node Update(GameTime gameTime)
        {
            _key = Keyboard.GetState();
            UpdateRect();

            _phaseRegister.IsLocked = true;
            _phasePool1.IsLocked = true;
            _phasePool2.IsLocked = true;
            _phaseDemiFinal.IsLocked = true;

            _prevScreenNode = CurrentScreenNode;

            if (_cameraX == -_phaseRegister._x) {_phaseRegister.IsLocked = false; CurrentScreenNode = _phaseRegister; Phase = Phases.None; }
            if (_cameraX == -_phasePool1._x) {_phasePool1.IsLocked = false; CurrentScreenNode = _phasePool1; Phase = Phases.Pool1; }
            if (_cameraX == -_phasePool2._x) {_phasePool2.IsLocked = false; CurrentScreenNode =_phasePool2; Phase = Phases.Pool2; }
            if (_cameraX == -_phaseDemiFinal._x) {_phaseDemiFinal.IsLocked = false; CurrentScreenNode =_phaseDemiFinal; Phase = Phases.DemiFinal; }


            if (_prevScreenNode != CurrentScreenNode)
            {
                Static.Server.SendUpdatePhaseToAll();
            }
            //
            //if (ButtonControl.OnePress("CopyTeam0", _key.IsKeyDown(Keys.C)))
            //{
            //    _phasePool2.GetTeam(0).SetStats(_phasePool1.GetTeam(0).Stats);
            //}

            if (ButtonControl.OnePress("SlideLeft", _key.IsKeyDown(Keys.Left) && _key.IsKeyDown(Keys.LeftControl)))
            {
                if (_cameraX < 0)
                {
                    //Misc.Log("Slide Left");

                    _animate.SetMotion("SlideLeft", Easing.QuadraticEaseOut, _cameraX, _cameraX = _cameraX + Screen.Width, 24);

                    _animate.Start("SlideLeft");

                    Static.SoundRanking.Play(.5f * Static.VolumeMaster, .5f, 0f);
                }

            }
            if (ButtonControl.OnePress("SlideRight", _key.IsKeyDown(Keys.Right) && _key.IsKeyDown(Keys.LeftControl)))
            {
                if (_cameraX > -Screen.Width * _nbScreen)
                {
                    //Misc.Log("Slide Right");

                    _animate.SetMotion("SlideRight", Easing.QuadraticEaseOut, _cameraX, _cameraX = _cameraX - Screen.Width, 24);

                    _animate.Start("SlideRight");

                    Static.SoundRanking.Play(.5f * Static.VolumeMaster, .5f, 0f);
                }
            }


            if (_animate.IsPlay())
            {
                SetPosition(_animate.Value(), 0);
            }

            _animate.NextFrame();

            UpdateChilds(gameTime);


            _scrolling.X -= 2f;

            if (_scrolling.X <= -_sizeTextScrolling )
                _scrolling.X = 0;


            return base.Update(gameTime);
        }
        public override Node Draw(SpriteBatch batch, GameTime gameTime, int indexLayer)
        {
            batch.GraphicsDevice.Clear(Color.Transparent);

            if (indexLayer == (int)Layers.HUD)
            {
                //batch.FilledCircle(Static.TexCircle, _scrolling + AbsXY, 40, Color.Yellow);
                batch.LeftMiddleString(_fontScrolling, _textScrolling + _textScrolling, _scrolling + Vector2.One * 4, Color.Black * .75f);
                batch.LeftMiddleString(_fontScrolling, _textScrolling + _textScrolling, _scrolling, _colorTextScrolling);

                batch.FillRectangleCentered(new Vector2(Screen.Width/2, 24), new Vector2(130, 40), Color.Black * .5f, 0);
                batch.LeftMiddleString(Static.FontMain, DateTime.Now.ToString("HH:mm:ss"), new Vector2(Screen.Width/2 - 56, 24), Color.Yellow);
            }

            if (indexLayer == (int)Layers.Debug)
            {
                //batch.RightMiddleString(Static.FontMicro, $"VolleyBall Tournament V{1}.{0} ©SilverTES 2025", _versionPos, Color.White);

                var clients = Static.Server.Clients;
                var clientXCourts = Static.Server.ClientControlCourts;

                if (clients.Count > 0)
                    for (int i = 0; i < clients.Count; i++)
                    {
                        batch.LeftMiddleString(Static.FontMicro, $"{i} : {clients[i].Id} : Controle le Terrain {clientXCourts[i]+1}", new Vector2(10, 80 + 32 * i), Color.Orange);
                    }

                batch.RightMiddleString(Static.FontMicro, $"CurrentScreenNode  = {CurrentScreenNode._name}", _versionPos, Color.Orange);
            }

            //batch.String(Static.FontMain, $"{_cameraX}", Vector2.One * 200, Color.White);
            //batch.String(Static.FontMain, $"{PhaseRegister.IsPaused} : {PhasePool1.IsPaused} : {PhasePool2.IsPaused}", Vector2.One * 200, Color.White);
            DrawChilds(batch, gameTime, indexLayer);


            return base.Draw(batch, gameTime, indexLayer);
        }
    }
}
