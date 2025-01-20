using System.Diagnostics;
using System.Drawing;
using DearImGui;
using DearImGui.OpenTK;
using DearImGui.OpenTK.Extensions;
using DearImPlot;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
//using Vector2 = System.Numerics.Vector2;
using Vector2 = OpenTK.Mathematics.Vector2;
using System.Threading;
using System.ComponentModel;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Runtime.CompilerServices;
using MathNet.Numerics.Distributions;

namespace SampleApplication.OpenTK;

// Struct for camera
struct ViewPerspectiveSettings
{
    public float fov, f, n;
    public ViewPerspectiveSettings(float Fov, float F, float N) { fov = Fov; f = F; n = N; }
}

internal sealed class MyGameWindow : GameWindowBaseWithDebugContext
{
    private static readonly double[] SampleData1 = Enumerable.Range(0, 256).Select(s => Math.Cos(s / 2.0d / Math.PI)).ToArray();

    private static readonly double[] SampleData2 = Enumerable.Range(0, 256).Select(s => Math.Sin(s / 2.0d / Math.PI)).ToArray();

    private readonly ImGuiController Controller;

    private readonly ImPlotContext ImPlotContext;

    private Color4 Color1 = Color4.Crimson;

    private Color4 Color2 = Color4.DeepSkyBlue;

    private bool ShowImGuiDemo = true;

    private bool ShowImPlotDemo = true;

    private bool ShowDockingDemo = true;

    // My code
    ViewPerspectiveSettings perspectiveSettings;
    Camera camera;
    Shader shader;
    Line line;
    Circle circle;
    float alfa; Thread thread;
    bool run = true;

    float len = 25f;
    float radius = 5f;
    float w = 60f; //obrót z predkościa w stopniach na sekunde (wewnatrz Line kat jest przerabiany na radiany)
    float eps = 0f;

    public const string ShaderVertLoc = "Shaders/shaderVert.hlsl";
    public const string ShaderFragLoc = "Shaders/shaderFrag.hlsl";

    List<float> time;
    List<float> posX;
    List<float> derX;
    List<float> derderX;

    float current_time = 0f;

    bool auto1 = true;
    bool auto2 = true;
    bool auto3 = true;
    bool auto4 = true;
    // End

    public MyGameWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
        : base(gameWindowSettings, nativeWindowSettings)
    {
        Controller = new ImGuiController(this, "Roboto-Regular.ttf", 17.0f);

        ImPlotContext = ImPlot.CreateContext();

        ImPlot.SetCurrentContext(ImPlotContext);

        ImPlot.SetImGuiContext(Controller.Context);

        // My code
        //Shader initialization
        shader = new Shader(ShaderVertLoc, ShaderFragLoc);

        //Camera initialization
        camera = new Camera();
        perspectiveSettings = new ViewPerspectiveSettings(45.0f, 30.0f, 0.5f);
        camera.UpdateProjectionMatrix((float)ClientSize.X, (float)ClientSize.Y, perspectiveSettings.fov, perspectiveSettings.n, perspectiveSettings.f);

        //Line initialization
        line = new Line(new Vector2(-5f, 0), new Vector2(5, 0));

        //Circle initialization
        circle = new Circle(radius);

        //Degree at start
        alfa = 0;

        //List of x, x', x'' and t 
        posX = new List<float>();
        time = new List<float>();
        derX = new List<float>();
        derderX = new List<float>();
        //time.Add(0.0f);
        //posX.Add(0.0f);

        //Start thread to update angle
        thread = new Thread(new ThreadStart(() => UpdateAngle(ref alfa, ref run, ref w, ref time, ref posX, ref radius, ref len, ref derX, ref derderX)));
        thread.Start();

        
        // End
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Controller.Dispose();
            ImPlot.DestroyContext(ImPlotContext);
        }

        base.Dispose(disposing);
    }

    public static void UpdateAngle(ref float alfa, ref bool run, ref float w, ref List<float> time, ref List<float> X, ref float R, ref float len, ref List<float> V, ref List<float> A)
    {
        float actual_time = 0f;
        float dt = 0.016f;
        while (run)
        {
            var startTime = DateTime.Now;

            alfa += w / (float)62.5;        // aktualizacja następuję co 16ms, czyli to jest 62.5 fps dlatego trzeba przez tyle podzielić wartość prędkości kątowej
            alfa = alfa % 360;

            time.Add(actual_time);
            actual_time += dt;

            var end = new Vector2((float)Math.Sin(alfa * (float)Math.PI / (float)180), (float)Math.Cos(alfa * (float)Math.PI / (float)180)) * R;
            var startX = end.X + (float)Math.Sqrt((double)(len * len - end.Y * end.Y));

            X.Add(startX);

            // Calculate v
            if (X.Count >= 2)
            {
                var lastVal = X[X.Count-1];
                var beforeLastVal = X[X.Count - 2];
                var vel = (lastVal - beforeLastVal) / dt;
                V.Add(vel);
            }

            // Calculate a
            if (V.Count >= 2)
            {
                var acc = (V[V.Count - 1] - V[V.Count - 2]) / dt;
                A.Add(acc);
            }

            while (DateTime.Now < startTime.AddMilliseconds(dt*1000)) ;
        }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        base.OnClosing(e);
        run = false;
        thread.Join();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        SampleExtraFontImpl();

        // Update len based on error eps 
        len = (float)(len + GenError(eps));

       

        Controller.Update((float)args.Time);
    }

    private double GenError(double eps)
    {
        var normalDistribution = new Normal(0, eps);
        return normalDistribution.Sample();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        GL.ClearColor(Color.CornflowerBlue);
        GL.Clear(ClearBufferMask.ColorBufferBit);

        //ImGui.SetNextWindowSize(new System.Numerics.Vector2(800, 400), ImGuiCond.Once);

        //if (ShowDockingDemo)
        //{
        //    DrawDockSpaceOptionsBar(ref ShowDockingDemo);
        //}






        if (ImGui.Begin("Wykres X"))
        {

            ImGui.Checkbox("Auto Fix", ref auto1);

            if(auto1)
            {
                ImPlot.SetNextAxesToFit();
            }
           
            if (ImPlot.BeginPlot("X(t)"))
            {
                //ImPlot.SetupAxes("t(s)", "X(t)");
                
                ImPlot.PlotLine("X(t)", ref time.ToArray()[0],ref posX.ToArray()[0], posX.ToArray().Length);

                ImPlot.EndPlot();
            }

        }

        ImGui.End();

        if(ImGui.Begin("Wykres V"))
        {
            ImGui.Checkbox("Auto Fix", ref auto2);

            if (auto2)
            {
                ImPlot.SetNextAxesToFit();
            }

            if(derX.Count > 1) 
            {
                if (ImPlot.BeginPlot("X'(t)"))
                {
                    //ImPlot.SetupAxes("t(s)", "X(t)");

                    ImPlot.PlotLine("X'(t)", ref time.ToArray()[0], ref derX.ToArray()[0], derX.ToArray().Length);

                    ImPlot.EndPlot();
                }
            }
        }
        ImGui.End();

        if (ImGui.Begin("Wykres A"))
        {
            ImGui.Checkbox("Auto Fix", ref auto3);

            if (auto3)
            {
                ImPlot.SetNextAxesToFit();
            }

            if (derderX.Count > 1)
            {
                if (ImPlot.BeginPlot("X''(t)"))
                {
                    //ImPlot.SetupAxes("t(s)", "X(t)");

                    ImPlot.PlotLine("X''(t)", ref time.ToArray()[0], ref derderX.ToArray()[0], derderX.ToArray().Length);

                    ImPlot.EndPlot();
                }
            }
        }
        ImGui.End();


        if (ImGui.Begin("Wykres V(X)"))
        {
            ImGui.Checkbox("Auto Fix", ref auto3);

            if (auto3)
            {
                ImPlot.SetNextAxesToFit();
            }

            if (derX.Count > 1)
            {
                if (ImPlot.BeginPlot("X'(X)"))
                {
                    //ImPlot.SetupAxes("t(s)", "X(t)");

                    ImPlot.PlotLine("X'(X)", ref posX.ToArray()[0], ref derX.ToArray()[0], derX.ToArray().Length);

                    ImPlot.EndPlot();
                }
            }
        }
        ImGui.End();

        //SampleExtraFontDemo();

        //if (ShowImGuiDemo)
        //{
        //    ImGui.ShowDemoWindow(ref ShowImGuiDemo);
        //}


        ImGui.Begin("Paramtery");
        if(ImGui.SliderFloat("Lenght", ref len, 15.0f, 40f))
        {
            line.UpdateEndPos(180, len, circle.radius);
        }
        if(ImGui.SliderFloat("R", ref radius, 1.0f, 10.0f))
        {
            circle.UpdateValue(radius);
        }
        if (ImGui.SliderFloat("w", ref w, 0.0f, 360.0f))
        {
            //circle.UpdateValue(radius);
        }
        if (ImGui.SliderFloat("eps", ref eps, 0.0f, 0.3f))
        {
            
        }
        ImGui.End();

        //if (ShowImPlotDemo)
        //{
        //    ImPlot.ShowDemoWindow(ref ShowImPlotDemo);
        //}

        line.UpdateEndPos(alfa, len, circle.radius);
        line.Draw(shader, camera.viewMatrix, camera.projectionMatrix);
        circle.Draw(shader, camera.viewMatrix, camera.projectionMatrix);

        //ImGui.Begin("Window A");
        //if (ImGui.BeginTabBar("Testing"))
        //{
        //    if (ImGui.BeginTabItem("Test1"))
        //    {
        //        if (ImGui.SliderFloat("X Value", ref len, 0.0f, 10f))
        //        {
        //            line.UpdateEndPos(180, len, circle.radius);
        //        }
        //        if (ImGui.SliderFloat("R Value", ref radius, 1.0f, 10.0f))
        //        {
        //            circle.UpdateValue(radius);
        //        }
        //        ImGui.EndTabItem();
        //    }
        //}
        //ImGui.End();

        Controller.Render();

        SwapBuffers();
    }

    protected override void OnResize(ResizeEventArgs e)
    {
        base.OnResize(e);

        GL.Viewport(0, 0, e.Width, e.Height);
    }

    #region SampleDocking

    private ImGuiDockNodeFlags dockspace_flags = ImGuiDockNodeFlags.None;

    void DrawDockSpaceOptionsBar(ref bool p_open)
    {
        ImGui.DockSpaceOverViewport(ImGui.GetMainViewport(), dockspace_flags);
        if (ImGui.BeginMainMenuBar())
        {
            if (ImGui.BeginMenu("Options"))
            {
                if (ImGui.MenuItem("Enable Docking", "IO.ConfigFlags.DockingEnable", ImGui.GetIO().ConfigFlags.HasFlag(ImGuiConfigFlags.DockingEnable))) {
                    ImGui.GetIO().ConfigFlags ^= ImGuiConfigFlags.DockingEnable;
                }

                ImGui.Separator();

                if (ImGui.MenuItem("Require Shift For Docking", "IO.ConfigDockingWithShift",
                    ImGui.GetIO().ConfigDockingWithShift)) { ImGui.GetIO().ConfigDockingWithShift = !ImGui.GetIO().ConfigDockingWithShift; }

                ImGui.Separator();

                if (ImGui.MenuItem("Flag: NoSplit", "", (dockspace_flags & ImGuiDockNodeFlags.NoSplit) != 0)) {  dockspace_flags ^= ImGuiDockNodeFlags.NoSplit; }
                if (ImGui.MenuItem("Flag: NoResize", "", (dockspace_flags & ImGuiDockNodeFlags.NoResize) != 0)) { dockspace_flags ^= ImGuiDockNodeFlags.NoResize; }
                if (ImGui.MenuItem("Flag: NoDockingInCentralNode", "", (dockspace_flags & ImGuiDockNodeFlags.NoDockingInCentralNode) != 0)) { dockspace_flags ^= ImGuiDockNodeFlags.NoDockingInCentralNode; }
                if (ImGui.MenuItem("Flag: AutoHideTabBar", "", (dockspace_flags & ImGuiDockNodeFlags.AutoHideTabBar) != 0)) { dockspace_flags ^= ImGuiDockNodeFlags.AutoHideTabBar; }
                if (ImGui.MenuItem("Flag: PassthruCentralNode", "", (dockspace_flags & ImGuiDockNodeFlags.PassthruCentralNode) != 0)) { dockspace_flags ^= ImGuiDockNodeFlags.PassthruCentralNode; }
                ImGui.EndMenu();
            }
            HelpMarker(
                @"When docking is enabled, you can ALWAYS dock MOST window into another! Try it now!
    
- Drag from window title bar or their tab to dock/undock.
    
- Drag from window menu button (upper-left button) to undock an entire node (all windows).
    
- Hold SHIFT to disable docking (if io.ConfigDockingWithShift == false, default)
    
- Hold SHIFT to enable docking (if io.ConfigDockingWithShift == true)");

            ImGui.EndMainMenuBar();
        }

        static void HelpMarker(string desc)
        {
            ImGui.TextDisabled("(?)");
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35.0f);
                ImGui.TextUnformatted(desc);
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }
    }

    

    #endregion

    #region SampleExtraFont

    private bool? SampleExtraFontFlag;

    private unsafe void SampleExtraFontImpl()
    {
        if (SampleExtraFontFlag is false)
            return;

        SampleExtraFontFlag = false;

        var io = ImGui.GetIO();

        var fonts = io.Fonts;

        var ranges = fonts.GlyphRangesDefault;

        var size = Controller.GetDpiScaledFontSize(22.0f);

        var font = fonts.AddFontFromFileTTF("Roboto-Regular.ttf", size, null, ref *ranges);

        Debug.WriteLine(font);

        fonts.Build();

        Controller.UpdateFontsTextureAtlas();
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);

        //if(this.MouseState.IsButtonPressed(MouseButton.Right))
        if (this.MouseState[MouseButton.Right])
        {
            var delta = e.Delta.Y;
            camera.ChangeDistance((float)(delta * 0.01f));
        }
    }

    private void SampleExtraFontDemo()
    {
        ImGui.SetNextWindowSize(new System.Numerics.Vector2(400, 100), ImGuiCond.Once);

        if (ImGui.Begin("Sample: extra font"))
        {
            if (SampleExtraFontFlag is false)
            {
                ImGui.Text("Change font from Tools/Style Editor.");
            }
            else
            {
                if (ImGui.Button("Click to add an extra font"))
                {
                    SampleExtraFontFlag = true;
                }
            }
        }

        ImGui.End();
    }

    #endregion
}