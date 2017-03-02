// prog


using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Timers;



using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;

using ClassDef;


namespace DirectX_Tutorial
{
    public class WinForm : System.Windows.Forms.Form
    {


        public clsRail[] rails;
        public clsLink[] links;
        public clsHook[] hooks;

        private Microsoft.DirectX.Direct3D.Device device;
        private System.ComponentModel.Container components = null;
        //private float angle = 0f;
        private const float cUpDownOffset = 43300;//400f;
        private const float cLeftRightOffset = 82200;//400f;
        private const float czoom = -135000; // -1000f
        private const float cGripperStepPluse = 458 / 2; // it is haved because it is always used as a radus
        private const float cStepPlusePerMS = 4.03956f; // <<<<<<<<<< how fast the machine is running - step pulse per millsecound
        private const float cRailWidth = 200;

        private float UpDownOffset = cUpDownOffset;
        private float LeftRightOffset = cLeftRightOffset;
        private float zoom = czoom;

        private CustomVertex.PositionColored[] vertices;
        private int startX = -10;
        private int startY = -10;

        private int[] indices;
        private IndexBuffer ib;
        private VertexBuffer[] vb_RedRail;
        private VertexBuffer vb_Link_Red;
        private VertexBuffer vb_Link_Green;
        private VertexBuffer vb_Link_Blue;
        private VertexBuffer vb_Link_Orange;
        private VertexBuffer vb_Link_White;

        private Microsoft.DirectX.DirectInput.Device keyb;
        private Microsoft.DirectX.DirectInput.Device mkeyb; //mouse

        //fps
        private int fpsRate = 0;
        private int fpsFrameCount = 0;
        private int fpsMillisecondsCount = 0;
        static private DateTime currentTime;
        private DateTime lastTime;

        // text crap
        private Microsoft.DirectX.Direct3D.Font text;

        // run time movement
        private DateTime runTime;
        private int SorterRun = 0;

        // time n motion
        private System.Timers.Timer aTimer;
        private float timerTickCount;
        private double timerMSCount;
        //private DateTime timerTickLast;
        private DateTime timerTickCurrent;
        private float timerDelay;


        // link rate
        private DateTime LinkRate_StartTime;
        private float LinkRate_CountPerSec = 0;
        private int LinkRate_Stop = 1;
        private double RunTimeMSCount = 0;
        private double RunTimeMSCountLast = 0;


        private int PulseRate_Stop = 0;
        private float PulseRate_CountPerSec = 0;


        // timer to drive motion
        private void InitTimer()
        {
            runTime = DateTime.Now; // used to track FPS
            //timerTickLast = DateTime.Now; // track motion decay

            timerMSCount = 0;
            timerDelay = 0;

            aTimer = new System.Timers.Timer(1000);
            aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);


            aTimer.Enabled = true;

            GC.KeepAlive(aTimer); // used to avoid issue with long running timers

        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            aTimer.Enabled = false;
            aTimer.Interval = 1;
            timerTickCurrent = DateTime.Now;
            timerTickCount = timerTickCount + 1;

            if (timerTickCount == 1)
            {
                runTime = DateTime.Now;
                aTimer.Enabled = true;
                return;
            }

            TimeSpan elapsedTimeSpan = timerTickCurrent.Subtract(runTime);
            //timerTickLast = timerTickCurrent;

            RunTimeMSCount = elapsedTimeSpan.TotalMilliseconds;

            timerMSCount = RunTimeMSCount - RunTimeMSCountLast;
            RunTimeMSCountLast = RunTimeMSCount;

            if (SorterRun == 1)
            {

                MoveLink(ref links, ref rails, ((float)timerMSCount * cStepPlusePerMS), links.Length);
                /*
                // bump all links
                for (int i = 0; i < links.Length; i++)
                {

                    //move it.
                    //links[i].center_loc_x = links[i].center_loc_x - (timerDelay * cStepPlusePerMS);
                    links[i].center_loc_x = links[i].center_loc_x - ((float)timerMSCount * cStepPlusePerMS);


                    //links[i].center_loc_y = links[i].center_loc_y + timerDelay;

                    // have we hit the end of the rail?
                    // uses backward logic as it travling in  a negative direction
                    if (links[i].center_loc_x < (rails[links[i].currentRail].lenght + rails[links[i].currentRail].start_Loc_x) * -1) // the mutliply by neg one it so make the rail lenth math the world quadracne that are all negative
                    {
                        links[i].center_loc_x = links[i].center_loc_x + rails[links[i].currentRail].lenght;
                        // links[i].center_loc_y = rails[links[i].currentRail].start_Loc_y;


                        if (LinkRate_Stop == 0)
                        {
                            if (LinkRate_CountPerSec == 0)
                            {
                                LinkRate_StartTime = DateTime.Now;
                            }
                            LinkRate_CountPerSec = LinkRate_CountPerSec + 1;

                        }
                    }

                }  
                */


            }

            if (LinkRate_CountPerSec > 0 && LinkRate_Stop == 0)
            {
                TimeSpan LinkRate_timespawn = DateTime.Now.Subtract(LinkRate_StartTime);
                if (LinkRate_timespawn.TotalMilliseconds >= 19999)
                {
                    LinkRate_Stop = 1;
                    LinkRate_CountPerSec = LinkRate_CountPerSec / ((float)LinkRate_timespawn.TotalMilliseconds / 1000f);
                }
            } //link rate

            if (PulseRate_Stop == 0)
            {

                PulseRate_CountPerSec = 1;

                if (PulseRate_CountPerSec > 0)
                {

                    if (RunTimeMSCountLast > 9999) //10 sec
                    {
                        PulseRate_Stop = 1;
                        PulseRate_CountPerSec = (float)RunTimeMSCountLast * cStepPlusePerMS / ((float)elapsedTimeSpan.TotalMilliseconds / 1000f);
                        timerDelay = (float)RunTimeMSCountLast / timerTickCount;
                    }
                }
            }// pulse rate


            aTimer.Enabled = true;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // WinForm
            // 
            this.ClientSize = new System.Drawing.Size(1700, 900);

            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "WinForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            this.Text = "SimSort2";
            this.ResumeLayout(false);

        }

        public WinForm()
        {

            vb_RedRail = new VertexBuffer[10];

            InitializeComponent();
            this.SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque, true);


            System.Diagnostics.Process myProcess = System.Diagnostics.Process.GetCurrentProcess();
            myProcess.PriorityClass = System.Diagnostics.ProcessPriorityClass.RealTime;


        }

        public void InitializeDevice()
        {
            PresentParameters presentParams = new PresentParameters();
            presentParams.Windowed = true;
            presentParams.SwapEffect = SwapEffect.Discard;

            device = new Microsoft.DirectX.Direct3D.Device(0, Microsoft.DirectX.Direct3D.DeviceType.Hardware, this, CreateFlags.HardwareVertexProcessing, presentParams);


            device.RenderState.CullMode = Cull.None;
            device.RenderState.FillMode = FillMode.Solid;
            //device.RenderState.ZBufferEnable = false;
            device.RenderState.Lighting = false;


            device.DeviceReset += new EventHandler(HandleResetEvent);
        }
        private void HandleResetEvent(object caller, EventArgs args)
        {
            device.RenderState.FillMode = FillMode.Solid;
            device.RenderState.CullMode = Cull.None;
            //device.RenderState.ZBufferEnable = false;
            device.RenderState.Lighting = false;
            CameraPositioning();
            VertexDeclarationRails();
            VertexDeclarationLinks();
            IndicesDeclaration();

            device.DeviceReset += new EventHandler(HandleResetEvent);

        }
        public void InitializeKeyboard()
        {
            keyb = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Keyboard);
            keyb.SetCooperativeLevel(this, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.NonExclusive);


            mkeyb = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Mouse);
            mkeyb.SetCooperativeLevel(this, CooperativeLevelFlags.Foreground | CooperativeLevelFlags.NonExclusive);



        }
        private void InitializeFont()
        {
            System.Drawing.Font systemfont = new System.Drawing.Font("Arial", 10f, FontStyle.Regular);
            text = new Microsoft.DirectX.Direct3D.Font(device, systemfont);
        }

        //build rails 
        private void VertexDeclarationRails()
        {


            int i = 0;


            for (i = 0; i < rails.Length; i++)
            {

                // set start point for rail zero
                if (i == 0)
                {
                    rails[i].start_Loc_x = startX;
                    rails[i].start_Loc_y = startY;
                }
                else
                {

                    //goto the end of the last rail
                    // get last riad direct so we know how to find its tail.
                    if (rails[i - 1].directionOfTravel == (int)ClassDef.clsRail.direction.right)
                    {
                        rails[i].start_Loc_x = rails[i - 1].start_Loc_x - rails[i - 1].lenght;// +(rails[i - 1].width * 1);
                        rails[i].start_Loc_y = rails[i - 1].start_Loc_y;// - (rails[i - 1].width * 1);
                    }
                    if (rails[i - 1].directionOfTravel == (int)ClassDef.clsRail.direction.left)
                    {
                        rails[i].start_Loc_x = rails[i - 1].start_Loc_x + rails[i - 1].lenght;// + (rails[i - 1].width * 1);
                        rails[i].start_Loc_y = rails[i - 1].start_Loc_y;// + (rails[i - 1].width * 1);
                    }
                    if (rails[i - 1].directionOfTravel == (int)ClassDef.clsRail.direction.up)
                    {
                        rails[i].start_Loc_x = rails[i - 1].start_Loc_x;// - (rails[i - 1].width * 1);
                        rails[i].start_Loc_y = rails[i - 1].start_Loc_y + rails[i - 1].lenght;// - (rails[i - 1].width * 1);
                    }
                    if (rails[i - 1].directionOfTravel == (int)ClassDef.clsRail.direction.down)
                    {
                        rails[i].start_Loc_x = rails[i - 1].start_Loc_x;// - (rails[i - 1].width * 1);
                        rails[i].start_Loc_y = rails[i - 1].start_Loc_y - rails[i - 1].lenght;// - (rails[i - 1].width * 1);
                    }

                }

                vb_RedRail[i] = new VertexBuffer(typeof(CustomVertex.PositionColored), 4, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);

                vertices = new CustomVertex.PositionColored[4]; // make a thing with four verts
                vertices[0].Color = Color.Red.ToArgb();
                vertices[1].Color = Color.Red.ToArgb();
                vertices[2].Color = Color.Red.ToArgb();
                vertices[3].Color = Color.Red.ToArgb();


                if (i == 100)
                {
                    vertices[0].Position = new Vector3(1100, 1100, 0f);
                    vertices[1].Position = new Vector3(1100, 10000, 0f);
                    vertices[2].Position = new Vector3(10000, 1100, 0f);
                    vertices[3].Position = new Vector3(10000, 10000, 0f);
                }
                else if (rails[i].directionOfTravel == (int)ClassDef.clsRail.direction.right)
                {

                    vertices[0].Position = new Vector3(rails[i].start_Loc_x, rails[i].start_Loc_y, 0f);
                    vertices[1].Position = new Vector3(rails[i].start_Loc_x - rails[i].lenght, rails[i].start_Loc_y, 0f);
                    vertices[2].Position = new Vector3(rails[i].start_Loc_x, rails[i].start_Loc_y - (rails[i].width * 2), 0f);
                    vertices[3].Position = new Vector3(rails[i].start_Loc_x - rails[i].lenght, rails[i].start_Loc_y - (rails[i].width * 2), 0f);

                }
                else if (rails[i].directionOfTravel == (int)ClassDef.clsRail.direction.left)
                {
                    vertices[0].Position = new Vector3(rails[i].start_Loc_x, rails[i].start_Loc_y, 0f);
                    vertices[1].Position = new Vector3(rails[i].start_Loc_x + rails[i].lenght, rails[i].start_Loc_y, 0f);
                    vertices[2].Position = new Vector3(rails[i].start_Loc_x, rails[i].start_Loc_y + (rails[i].width * 2), 0f);
                    vertices[3].Position = new Vector3(rails[i].start_Loc_x + rails[i].lenght, rails[i].start_Loc_y + (rails[i].width * 2), 0f);

                }
                else if (rails[i].directionOfTravel == (int)ClassDef.clsRail.direction.up)
                {
                    vertices[0].Position = new Vector3(rails[i].start_Loc_x, rails[i].start_Loc_y, 0f);
                    vertices[1].Position = new Vector3(rails[i].start_Loc_x, rails[i].start_Loc_y + rails[i].lenght, 0f);
                    vertices[2].Position = new Vector3(rails[i].start_Loc_x - (rails[i].width * 2), rails[i].start_Loc_y, 0f);
                    vertices[3].Position = new Vector3(rails[i].start_Loc_x - (rails[i].width * 2), rails[i].start_Loc_y + rails[i].lenght, 0f);
                }
                else if (rails[i].directionOfTravel == (int)ClassDef.clsRail.direction.down)
                {

                    vertices[0].Position = new Vector3(rails[i].start_Loc_x, rails[i].start_Loc_y, 0f);
                    vertices[1].Position = new Vector3(rails[i].start_Loc_x, rails[i].start_Loc_y - rails[i].lenght, 0f);
                    vertices[2].Position = new Vector3(rails[i].start_Loc_x - (rails[i].width * 2), rails[i].start_Loc_y, 0f);
                    vertices[3].Position = new Vector3(rails[i].start_Loc_x - (rails[i].width * 2), rails[i].start_Loc_y - rails[i].lenght, 0f);

                }

                vb_RedRail[i].SetData(vertices, 0, LockFlags.None);
            }
        }


        // build links
        private void VertexDeclarationLinks()
        {

            vertices = new CustomVertex.PositionColored[5];

            /* conner layout
             *   A---B
             *     / 
             *   C---D
             * 
             */

            int widthOfLink = 1;
            int mGripperStepPluse = (int)(cGripperStepPluse * 0.7f);

            vertices[0].Position = new Vector3(rails[0].start_Loc_x + mGripperStepPluse, rails[0].start_Loc_y - rails[0].width + (mGripperStepPluse * widthOfLink), 0f);  // corner A
            vertices[1].Position = new Vector3(rails[0].start_Loc_x - mGripperStepPluse, rails[0].start_Loc_y - rails[0].width + (mGripperStepPluse * widthOfLink), 0f);  // conner B
            vertices[2].Position = new Vector3(rails[0].start_Loc_x + mGripperStepPluse, rails[0].start_Loc_y - rails[0].width - (mGripperStepPluse * widthOfLink), 0f);  // conner C
            vertices[3].Position = new Vector3(rails[0].start_Loc_x - mGripperStepPluse, rails[0].start_Loc_y - rails[0].width - (mGripperStepPluse * widthOfLink), 0f);  // conner D
            //white
            vertices[0].Color = Color.White.ToArgb();
            vertices[1].Color = Color.White.ToArgb();
            vertices[2].Color = Color.White.ToArgb();
            vertices[3].Color = Color.White.ToArgb();

            vb_Link_White = new VertexBuffer(typeof(CustomVertex.PositionColored), 5, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            vb_Link_White.SetData(vertices, 0, LockFlags.None);


            //blue
            vertices[0].Color = Color.Blue.ToArgb();
            vertices[1].Color = Color.Blue.ToArgb();
            vertices[2].Color = Color.Blue.ToArgb();
            vertices[3].Color = Color.Blue.ToArgb();

            vb_Link_Blue = new VertexBuffer(typeof(CustomVertex.PositionColored), 5, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            vb_Link_Blue.SetData(vertices, 0, LockFlags.None);

            //red
            vertices[0].Color = Color.Red.ToArgb();
            vertices[1].Color = Color.Red.ToArgb();
            vertices[2].Color = Color.Red.ToArgb();
            vertices[3].Color = Color.Red.ToArgb();

            vb_Link_Red = new VertexBuffer(typeof(CustomVertex.PositionColored), 5, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            vb_Link_Red.SetData(vertices, 0, LockFlags.None);

            //Green
            vertices[0].Color = Color.Green.ToArgb();
            vertices[1].Color = Color.Green.ToArgb();
            vertices[2].Color = Color.Green.ToArgb();
            vertices[3].Color = Color.Green.ToArgb();

            vb_Link_Green = new VertexBuffer(typeof(CustomVertex.PositionColored), 5, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            vb_Link_Green.SetData(vertices, 0, LockFlags.None);

            //Orange
            vertices[0].Color = Color.Orange.ToArgb();
            vertices[1].Color = Color.Orange.ToArgb();
            vertices[2].Color = Color.Orange.ToArgb();
            vertices[3].Color = Color.Orange.ToArgb();

            vb_Link_Orange = new VertexBuffer(typeof(CustomVertex.PositionColored), 5, device, Usage.Dynamic | Usage.WriteOnly, CustomVertex.PositionColored.Format, Pool.Default);
            vb_Link_Orange.SetData(vertices, 0, LockFlags.None);

        }

        private void IndicesDeclaration()
        {
            ib = new IndexBuffer(typeof(int), 6, device, Usage.WriteOnly, Pool.Default);
            indices = new int[6];

            indices[0] = 0; indices[1] = 1; indices[2] = 2;
            indices[3] = 1; indices[4] = 3; indices[5] = 2;

            ib.SetData(indices, 0, LockFlags.None);

            device.VertexFormat = CustomVertex.PositionColored.Format;
            device.Indices = ib;
        }

        // screeen rebuild
        protected override void OnPaint(System.Windows.Forms.PaintEventArgs e)
        {
            Paint();
        }
        protected void Paint()
        {

            float xOffset = 0;
            float yOffset = 0;
            int i = 0;

            currentTime = DateTime.Now;
            TimeSpan elapsedTimeSpan = currentTime.Subtract(lastTime);
            lastTime = currentTime;

            fpsFrameCount += 1;
            fpsMillisecondsCount += elapsedTimeSpan.Milliseconds;

            if (fpsMillisecondsCount >= 1000)
            {
                fpsRate = fpsFrameCount / (fpsMillisecondsCount / 1000);
                fpsMillisecondsCount = 0;
                fpsFrameCount = 0;

            }

            // motion

            TimeSpan runTimeSpan = currentTime.Subtract(runTime);


            device.Clear(ClearFlags.Target, Color.Black, 1.0f, 0);
            device.BeginScene();


            for (i = 0; i < rails.Length; i++)
            {
                device.SetStreamSource(0, vb_RedRail[i], 0);

                device.Transform.World = Matrix.Translation((0) + LeftRightOffset - (1 * 7), (0) + UpDownOffset, zoom);
                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 100, 0, indices.Length / 3);


            }


            text.DrawText(null, string.Format("fps: {0}", fpsRate), new Point(10, 20), Color.White);
            text.DrawText(null, string.Format("scan rate: {0} ms", timerDelay), new Point(10, 40), Color.White);
            text.DrawText(null, string.Format("pluse per sec: {0}", PulseRate_CountPerSec), new Point(10, 60), Color.White);
            text.DrawText(null, string.Format("link per sec: {0}", LinkRate_CountPerSec), new Point(10, 80), Color.White);






            for (i = 0; i < links.Length; i++)
            {

                xOffset = LeftRightOffset + links[i].center_loc_x;// -(i * (cGripperStepPluse * 2));
                yOffset = UpDownOffset + links[i].center_loc_y;

                if (i == 0)
                {
                    device.SetStreamSource(0, vb_Link_Blue, 0);
                }
                else if (i == 99)
                {
                    device.SetStreamSource(0, vb_Link_Green, 0);
                }
                else if (i == 199)
                {
                    device.SetStreamSource(0, vb_Link_Orange, 0);
                }
                //else if (i == 3)
                //{
                //    device.SetStreamSource(0, vb_Link_Orange, 0);
                //}
                else
                {
                    device.SetStreamSource(0, vb_Link_White, 0);
                }

                // handle orientation 
                if (rails[links[i].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.right)
                {

                    device.Transform.World = Matrix.Translation(xOffset, yOffset, zoom);
                }
                else if (rails[links[i].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.left)
                {

                    device.Transform.World = Matrix.Translation(xOffset, yOffset, zoom);
                }
                else if (rails[links[i].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.up)
                {
                    // device.Transform.World = Matrix.RotationZ(90);
                    device.Transform.World = Matrix.Translation(xOffset, yOffset, zoom);
                }
                else if (rails[links[i].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.down)
                {
                    device.Transform.World = Matrix.Translation(xOffset, yOffset, zoom);// *Matrix.RotationZ(90f);

                }

                //device.Transform.World = Matrix.Translation(xOffset, yOffset, zoom);


                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 100, 0, indices.Length / 3);
            }





            device.EndScene();
            device.Present();
            this.Invalidate();

            ReadKeyboard();
        }
        private void CameraPositioning()
        {
            device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4, (float)this.Width / (float)this.Height, 1500f, 1500000f);
            //device.Transform.Projection = Matrix.PerspectiveFovLH((float)Math.PI / 4,1, 1f, 15000f);
            device.Transform.View = Matrix.LookAtLH(new Vector3(0, 0, 50), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            device.RenderState.Lighting = false;
            device.RenderState.CullMode = Cull.None;
        }

        // hot keys defined
        private void ReadKeyboard()
        {

            try
            {
                keyb.Acquire();
                KeyboardState keys = keyb.GetCurrentKeyboardState();

                if (keys[Key.RightArrow] || keys[Key.A])
                {
                    //LeftRightOffset += 1000;
                    LeftRightOffset += (zoom) / 40f;
                }
                if (keys[Key.LeftArrow] || keys[Key.D])
                {
                    // LeftRightOffset -= 1000;
                    LeftRightOffset -= (zoom) / 40f;
                }

                if (keys[Key.UpArrow] || keys[Key.W])
                {
                    UpDownOffset -= (zoom) / 40f;
                }
                if (keys[Key.DownArrow] || keys[Key.S])
                {
                    UpDownOffset += (zoom) / 40f;
                } // reset 
                if (keys[Key.NumPad1])
                {
                    //UpDownOffset = 400;
                    //LeftRightOffset = 800;
                    //zoom = -1000;

                    UpDownOffset = cUpDownOffset;
                    LeftRightOffset = cLeftRightOffset;
                    zoom = czoom;



                    //runTimeMs += 1;
                }
                if (keys[Key.NumPad2])
                {
                    LeftRightOffset = 12644;
                    UpDownOffset = 10159;
                    zoom = -25313;

                }
                if (keys[Key.NumPad3])
                {
                    LeftRightOffset = 120994;
                    UpDownOffset = 10125;
                    zoom = -27287;

                }

                if (keys[Key.NumPad9])
                {
                    SorterRun = 1;

                    // turn on stat loger for links per sec
                    LinkRate_Stop = 0;
                    LinkRate_CountPerSec = 0;
                }
                if (keys[Key.NumPad8])
                {
                    SorterRun = 0;
                }

            }
            catch
            {
            }

            //MouseState CurrentMouseState
            double d;
            try
            {
                mkeyb.Acquire();
                MouseState mKeys = mkeyb.CurrentMouseState;
                byte[] b = mKeys.GetMouseButtons();


                if (mKeys.Z > 0)
                {
                    //zoom += 5000;
                    d = zoom;
                    zoom -= (int)zoom * 0.05f;
                }
                if (mKeys.Z < 0)
                {
                    //zoom -= 5000;
                    zoom += (int)zoom * 0.05f;
                }

                if (b[0] == 128) // left click
                {
                    //UpDownOffset = cUpDownOffset;
                    //LeftRightOffset = cLeftRightOffset;
                    //zoom = czoom;
                }

                //mouse_x = mKeys.x;


            }
            catch
            {
            }



        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private void PopulateRailData()
        {

            int i = 0;

            rails = new clsRail[5];
            for (i = 0; i < rails.Length; i++)
            {
                rails[i] = new clsRail();
            }

            i = 0;
            rails[i].railID = 0;
            rails[i].railChild = 1;
            rails[i].railType = 1;
            //rails[i].start_Loc_x = -10;
            //rails[i].start_Loc_y = -10;
            rails[i].lenght = 10000;
            rails[i].width = cRailWidth; // this is 1/2 of final width
            rails[i].directionOfTravel = (int)ClassDef.clsRail.direction.left;

            i++;
            rails[i].railID = 1;
            rails[i].railChild = 2;
            rails[i].railType = 1;
            //rails[i].start_Loc_x = rails[i - 1].start_Loc_x - rails[i - 1].lenght;
            //rails[i].start_Loc_y = rails[i - 1].start_Loc_y - (rails[i - 1].width * 2);
            rails[i].lenght = 10000;
            rails[i].width = cRailWidth;
            rails[i].directionOfTravel = (int)ClassDef.clsRail.direction.up;

            i++;
            rails[i].railID = 2;
            rails[i].railChild = 3;
            rails[i].railType = 1;
            //rails[i].start_Loc_x = -10;
            //rails[i].start_Loc_y = -10;
            rails[i].lenght = 10000;
            rails[i].width = cRailWidth;
            rails[i].directionOfTravel = (int)ClassDef.clsRail.direction.right;

            //rail 4 closes box
            i++;
            rails[i].railID = 3;
            rails[i].railChild = 4;
            rails[i].railType = 1;
            //rails[i].start_Loc_x = -10;
            // rails[i].start_Loc_y = -10;
            rails[i].lenght = 10000;
            rails[i].width = cRailWidth;
            rails[i].directionOfTravel = (int)ClassDef.clsRail.direction.down;

            //rail 5
            i++;
            rails[i].railID = 4;
            rails[i].railChild = 0;
            rails[i].railType = 1;
            //rails[i].start_Loc_x = -10;
            // rails[i].start_Loc_y = -10;
            rails[i].lenght = 10000;
            rails[i].width = cRailWidth;
            rails[i].directionOfTravel = (int)ClassDef.clsRail.direction.right;
        }
        private void PopulateLinkData()
        {

            int i = 0;

            // link
            // white - AVL
            // gray - blocked
            // yellow used

            // 1 = red
            // 2 = green
            // 3 = blue
            // 4 = gray
            // 5 = white
            // 6 = orange
            // 7 = pink
            // 8 = brown

            links = new clsLink[100];
            for (i = 0; i < links.Length; i++)
            {

                // move the current chain down one link to make room for the new link

                if (i > 0)// skip the first link being created as there are no other links to move out of its way, aka not need for first link
                {
                    MoveLink(ref links, ref rails, (cGripperStepPluse * 2f), i);
                }


                links[i] = new clsLink();

                // add new link to world
                links[i].center_loc_x = startX;
                links[i].center_loc_y = startY;
                links[i].currentRail = 0;
                links[i].color = (int)ClassDef.clsLink.ColorStatus.available;





            }




        }

        static void Main()
        {


            using (WinForm our_directx_form = new WinForm())
            {


                our_directx_form.InitTimer();

                our_directx_form.Show();
                our_directx_form.InitializeDevice();
                our_directx_form.CameraPositioning();

                // build rails
                our_directx_form.PopulateRailData();
                our_directx_form.VertexDeclarationRails();

                //build links
                our_directx_form.PopulateLinkData();
                our_directx_form.VertexDeclarationLinks();

                our_directx_form.IndicesDeclaration();

                our_directx_form.InitializeKeyboard();
                our_directx_form.InitializeFont();

                Application.Run(our_directx_form);

            }
        }



        //private void MoveLink( float Pulses2Add)
        private void MoveLink(ref  clsLink[] l, ref  clsRail[] r, float Pulses2Add, int count)
        {
            float Pulses4ThisLink = 0;
            // move links from tail to head
            for (int n = count - 1; n > -1; n--)
            {
                Pulses4ThisLink = Pulses2Add;

                // 1. get current rail dir of travel
                // 2. get end point
                // 3. check if currentlink will excead end of rail
                // 4. if so addjust amount of travel to user whats left of rail and change current rail to next rail


                if (r[l[n].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.right)
                {
                    // because we work in a negative quadant < realy means > as far as normal logic reads.
                    if (links[n].center_loc_x - Pulses4ThisLink < (rails[links[n].currentRail].lenght + rails[links[n].currentRail].start_Loc_x) * -1) // the mutliply by neg one it so make the rail lenth math the world quadracne that are all negative
                    {

                        links[n].center_loc_y = links[n].center_loc_x + rails[links[n].currentRail].lenght;

                        //bump current rail
                        l[n].currentRail = (r[l[n].currentRail].railChild);


                    }
                }
                else if (r[l[n].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.left)
                {
                    // because we work in a negative quadant < realy means > as far as normal logic reads.
                    if (links[n].center_loc_x + Pulses4ThisLink > (rails[links[n].currentRail].start_Loc_x + rails[links[n].currentRail].lenght) * -1) // the mutliply by neg one it so make the rail lenth math the world quadracne that are all negative
                    {
                        //bump current rail
                        links[n].center_loc_y = r[l[n].currentRail].start_Loc_y - (r[l[n].currentRail].width * -2) + links[n].center_loc_x;// + rails[links[n].currentRail].lenght);
                        l[n].currentRail = (r[l[n].currentRail].railChild);

                    }
                }
                else if (r[l[n].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.up)
                {
                    // because we work in a negative quadant < realy means > as far as normal logic reads.
                    if (links[n].center_loc_y + Pulses4ThisLink > (r[l[n].currentRail].width * -2) + (rails[links[n].currentRail].lenght + rails[links[n].currentRail].start_Loc_y) * -1) // the mutliply by neg one it so make the rail lenth math the world quadracne that are all negative
                    {
                        //bump current rail
                        links[n].center_loc_x = r[l[n].currentRail].start_Loc_x - (links[n].center_loc_y) + (r[l[n].currentRail].width * -2);// <--- this is wrong
                        l[n].currentRail = (r[l[n].currentRail].railChild);
                    }
                }
                else if (r[l[n].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.down)
                {
                    // because we work in a negative quadant < realy means > as far as normal logic reads.
                    if (links[n].center_loc_y - Pulses4ThisLink < (rails[links[n].currentRail].lenght + rails[links[n].currentRail].start_Loc_y) * -1) // the mutliply by neg one it so make the rail lenth math the world quadracne that are all negative
                    {
                        //bump current rail
                        links[n].center_loc_x = r[l[n].currentRail].start_Loc_x - (links[n].center_loc_y + rails[links[n].currentRail].lenght);
                        l[n].currentRail = (r[l[n].currentRail].railChild);

                        // diag stat stuff
                        if (LinkRate_Stop == 0)
                        {
                            if (LinkRate_CountPerSec == 0)
                            {
                                LinkRate_StartTime = DateTime.Now;
                            }
                            LinkRate_CountPerSec = LinkRate_CountPerSec + 1;
                        }
                    }
                }


                // 5. get get current rail driver of travel --  this may have changed from the first time to now which is why its done twice 
                // 6. move link for whats left of pulse4links

                // get move in correct direction
                if (r[l[n].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.right)
                {
                    links[n].center_loc_x = links[n].center_loc_x - Pulses4ThisLink;
                    links[n].center_loc_y = r[l[n].currentRail].start_Loc_y;
                }
                else if (r[l[n].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.left)
                {
                    links[n].center_loc_x = links[n].center_loc_x + Pulses4ThisLink;
                    links[n].center_loc_y = r[l[n].currentRail].start_Loc_y;// +(cGripperStepPluse * 1.8f);
                }
                else if (r[l[n].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.up)
                {
                    links[n].center_loc_x = r[l[n].currentRail].start_Loc_x;// -(cGripperStepPluse * 0.85f);
                    links[n].center_loc_y = links[n].center_loc_y + Pulses4ThisLink;
                }
                else if (r[l[n].currentRail].directionOfTravel == (int)ClassDef.clsRail.direction.down)
                {
                    links[n].center_loc_x = r[l[n].currentRail].start_Loc_x;// -(cGripperStepPluse * 0.85f);
                    links[n].center_loc_y = links[n].center_loc_y - Pulses4ThisLink;
                }


            }
        }


    }
}
// prog