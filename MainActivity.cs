using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using Android.Media;
using Xamarin.Essentials;
using System;

namespace Exam_XamarinAnd
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        #region Les variables globales
        MediaPlayer player;
        // Set speed delay for monitoring changes.
        SensorSpeed speed = SensorSpeed.UI;

        public static TextView tvTxt;

        //Compter le nombre de choc, seconde sans mvt, nb Deplacement, nb Roullis & Tangage
        int countShock, secondeMvt, nbDeplacement, nbTangRoul, secTangRoulEnCours;
        //variable pour la voix
        string voix = "voix1";
        //ancienne variation
        float oldX, oldY, oldZ;
        float oldOrtX, oldOrtY;
        #endregion

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            tvTxt = FindViewById<TextView>(Resource.Id.tvTxt);

           
            //on choise la voix
            Button btnVoix1 = FindViewById<Button>(Resource.Id.btnVoix1);
            btnVoix1.Click += (sender, e) => {
                voix = "voix1";
            };
            Button btnVoix2 = FindViewById<Button>(Resource.Id.btnVoix2);
            btnVoix2.Click += (sender, e) => {
                voix = "voix2";
            };
            Button btnVoixOff = FindViewById<Button>(Resource.Id.btnVoixOff);
            btnVoixOff.Click += (sender, e) => {
                voix = "voixOff";
            };

            //on lance les toggle
            ToggleAccelerometer();
            //ToggleOrientationSensor();
            startTimer();
            

        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        #region Les ReadChangeds
        void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
        {
            //sons de démarage 1er deplacement
            if (nbDeplacement == 0 && (Math.Abs(Math.Round(oldX, 1)) > 0.1 || Math.Abs(Math.Round(oldY, 1)) > 0.1 || Math.Abs(Math.Round(oldZ, 2)) > 1.10))
            {
                nbDeplacement++;
                PlayerVoice(voix, 1);
            }

            float vrtX, vrtY, vrtZ;
            double seuil =1.8;


            // Process Acceleration X, Y, and z
            vrtX = e.Reading.Acceleration.X - oldX;
            vrtY = e.Reading.Acceleration.Y - oldY;
            vrtZ = e.Reading.Acceleration.Z - oldZ;

            var moy = (vrtX + vrtY + vrtZ) / 3;

            oldX = e.Reading.Acceleration.X;
            oldY = e.Reading.Acceleration.Y;
            oldZ = e.Reading.Acceleration.Z;


            if (moy >= seuil && nbDeplacement > 1)
            {
                countShock++;
                switch (countShock)
                {
                    case 1:
                        PlayerVoice(voix, 5);
                        break;
                    case 2:
                        PlayerVoice(voix, 6);
                        break;                        
                    default:
                        PlayerVoice(voix, 7);
                        break;                  
                }
            }  

        }


        void OrientationSensor_ReadingChanged(object sender, OrientationSensorChangedEventArgs e)
        {
            double seuil = 0.4;

            oldOrtX = e.Reading.Orientation.X;
            oldOrtY = e.Reading.Orientation.Y;

            if ((Math.Abs(Math.Round(oldOrtX,2)) == seuil || Math.Abs(Math.Round(oldOrtY, 2)) == seuil) && nbDeplacement >= 1)
            {
                nbTangRoul++;
                nbDeplacement++;

                switch (nbTangRoul)
                {
                    case 1:
                        PlayerVoice(voix, 2);
                        break;

                    case 5:
                        PlayerVoice(voix, 4);
                        break;
                }
            }

            

        }
        #endregion

        #region Les Toggles
        public void ToggleOrientationSensor()
        { 
            if (OrientationSensor.IsMonitoring)
            {
                OrientationSensor.ReadingChanged += OrientationSensor_ReadingChanged;
                OrientationSensor.Stop();
            }
            else
            {
                OrientationSensor.ReadingChanged += OrientationSensor_ReadingChanged;
                OrientationSensor.Start(speed);
            }
        }


        void ToggleAccelerometer()
        {
            if (Accelerometer.IsMonitoring)
            {
                Accelerometer.ReadingChanged -= Accelerometer_ReadingChanged;
                Accelerometer.Stop();
            }
            else
            {
                Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
                Accelerometer.Start(speed);

            }
        }
        #endregion

        #region Le Timer
        public void startTimer()
        {
            
                System.Timers.Timer Timer1 = new System.Timers.Timer();
                Timer1.Start();
                Timer1.Interval = 1000;
                Timer1.Enabled = true;
                Timer1.Elapsed += (object sender, System.Timers.ElapsedEventArgs e) =>
                {
                    RunOnUiThread(() =>
                    {                  

                        if (nbDeplacement > 1)
                        {
                            if (Math.Abs(oldOrtX) > 0.2 || Math.Abs(oldOrtY) > 0.1)
                            {
                                secTangRoulEnCours++;

                                if (secTangRoulEnCours == 5)
                                {
                                    PlayerVoice(voix, 3);
                                    //secondeMvt = 0;
                                }
                            }
                            else
                            {
                                secTangRoulEnCours = 0;
                            }

                            //remet à 0 secondeMvt si y à coup
                            if (Math.Abs(oldX) < 0.1 && Math.Abs(oldY) < 0.1 && Math.Abs(oldZ) < 1.1)
                            {
                                this.secondeMvt++;

                                if (this.secondeMvt == 5 && this.secondeMvt % 5 == 0 && this.secTangRoulEnCours != 5)
                                    PlayerVoice(voix, 8);

                                if (this.secondeMvt > 5 && (this.secondeMvt % 15) == 0)
                                    PlayerVoice(voix, 9);
                            }
                            else
                            {
                                this.secondeMvt = 0;
                            }




                        }



                    });
                };
            
            
        }
        #endregion

        #region Le player voice
        void PlayerVoice(string voix, int numVoix)
        {
            switch (voix)
            {
                case "voix1":
                    switch (numVoix)
                    {
                        case 1:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice01_01);
                            player.Start();
                            break;
                        case 2:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice01_02);
                            player.Start();
                            break;
                        case 3:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice01_03);
                            player.Start();
                            break;
                        case 4:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice01_04);
                            player.Start();
                            break;
                        case 5:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice01_05);
                            player.Start();
                            break;
                        case 6:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice01_06);
                            player.Start();
                            break;
                        case 7:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice01_07);
                            player.Start();
                            break;
                        case 8:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice01_08);
                            player.Start();
                            break;
                        case 9:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice01_09);
                            player.Start();
                            break;
                        case 10:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice01_10);
                            player.Start();
                            break;
                    }
                    
                    break;

                case "voix2":
                    switch (numVoix)
                    {
                        case 1:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice02_01);
                            player.Start();
                            break;
                        case 2:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice02_02);
                            player.Start();
                            break;
                        case 3:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice02_03);
                            player.Start();
                            break;
                        case 4:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice02_04);
                            player.Start();
                            break;
                        case 5:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice02_05);
                            player.Start();
                            break;
                        case 6:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice02_06);
                            player.Start();
                            break;
                        case 7:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice02_07);
                            player.Start();
                            break;
                        case 8:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice02_08);
                            player.Start();
                            break;
                        case 9:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice02_09);
                            player.Start();
                            break;
                        case 10:
                            player = MediaPlayer.Create(this, Resource.Raw.Voice02_10);
                            player.Start();
                            break;
                    }
                    break;
                default:
                    //vibreur
                    break;
            }

        }
        #endregion

    }
}