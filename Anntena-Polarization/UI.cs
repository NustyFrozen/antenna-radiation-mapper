using ClickableTransparentOverlay;
using Emgu.CV.Flann;
using Emgu.CV.Structure;
using ImGuiNET;
using SharpGen.Runtime;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Vortice.Mathematics;
using static Anntena_Polarization.Polarizer;
using static Emgu.Util.Platform;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace Anntena_Polarization
{
    internal class UI : ClickableTransparentOverlay.Overlay
    {
        public Bitmap origin,origin_dark;
        public Bitmap results;
        public nint originsrc, originsrc_dark, resultssrc;
        public void adjustBrightness()
        {
            origin_dark = new Bitmap(origin.Width, origin.Height);
            float brightness = 0.4f; // no change in brightness
            float contrast = 1.0f; // twice the contrast
            float gamma = 1.0f; // no change in gamma

            float adjustedBrightness = brightness - 1.0f;
            // create matrix that will brighten and contrast the image
            float[][] ptsArray ={
        new float[] {contrast, 0, 0, 0, 0}, // scale red
        new float[] {0, contrast, 0, 0, 0}, // scale green
        new float[] {0, 0, contrast, 0, 0}, // scale blue
        new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
        new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

            ImageAttributes imageAttributes = new ImageAttributes();
            imageAttributes.ClearColorMatrix();
            imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
            imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
            Graphics g = Graphics.FromImage(origin_dark);
            g.DrawImage(origin, new Rectangle(0, 0, origin_dark.Width, origin_dark.Height)
                , 0, 0, origin.Width, origin.Height,
                GraphicsUnit.Pixel, imageAttributes);
        }
        public void loadresources()
        {
            AddOrGetImagePointer("origin", Polarizer.ConvertBitmapToImageSharp(origin), true, out originsrc);
            offset = new Vector2(20,ImGui.GetWindowSize().Y - origin.Size.Height);
            center = new Vector2(origin.Size.Width/2, origin.Size.Height / 2);
            adjustBrightness();
            AddOrGetImagePointer("origin_dark", Polarizer.ConvertBitmapToImageSharp(origin_dark), true, out originsrc_dark);
        }
        public UI() : base(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height) { }
        bool loadedResources = false;

        int scaleSize = 20;
        int start = -40;
        int scalePX = 20;
        Vector2 center = new Vector2();
        int state = 0;
        // 0 NONE
        // 1 center
        // 2 scale
        // 3 pick color
        Vector3 minrgb,maxrgb;
        Vector2 offset = new Vector2(10,60);
        void drawCenterPicker(Vector4 rect)
        {
            var draw = ImGui.GetWindowDrawList();
            Vector2 pos = ImGui.GetMousePos();
            if (pos.X < ImGui.GetWindowPos().X + offset.X)
                pos.X = ImGui.GetWindowPos().X + offset.X;
            if (pos.Y < ImGui.GetWindowPos().Y + offset.Y)
                pos.Y = ImGui.GetWindowPos().Y + offset.Y;
            if (pos.X > ImGui.GetWindowPos().X + rect.Z + offset.X)
                pos.X = ImGui.GetWindowPos().X + rect.Z + offset.X;
            if (pos.Y > ImGui.GetWindowPos().Y + rect.W + offset.Y)
                pos.Y = ImGui.GetWindowPos().Y + rect.W + offset.Y;
            center = pos - offset;
            draw.AddLine(new Vector2(ImGui.GetWindowPos().X + offset.X, pos.Y), new Vector2(ImGui.GetWindowPos().X + offset.X + rect.Z, pos.Y), 0xff000000,1);
            draw.AddLine(new Vector2(pos.X, ImGui.GetWindowPos().Y + offset.Y), new Vector2(pos.X, ImGui.GetWindowPos().Y + offset.Y + rect.W), 0xff000000, 1);
            if(ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                state = 0;
            }
        }
        void drawScalePicker(Vector4 rect)
        {
            var draw = ImGui.GetWindowDrawList();
            Vector2 pos = ImGui.GetMousePos();
            if (pos.X < ImGui.GetWindowPos().X + offset.X)
                pos.X = ImGui.GetWindowPos().X + offset.X;
            if (pos.X > ImGui.GetWindowPos().X + center.X + offset.X)
                pos.X = ImGui.GetWindowPos().X + center.X + offset.X;
            draw.AddLine(new Vector2(pos.X, ImGui.GetWindowPos().Y + offset.Y + center.Y), new Vector2(center.X, ImGui.GetWindowPos().Y + offset.Y + center.Y), 0xff000000, 1);
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                state = 0;
            }
            scalePX = Convert.ToInt32(center.X - pos.X) + 20;
        }
        void drawColorPicker(Vector4 rect)
        {
            try
            {
                var draw = ImGui.GetWindowDrawList();
                Vector2 pos = ImGui.GetMousePos();
                if (pos.X < ImGui.GetWindowPos().X + offset.X)
                    pos.X = ImGui.GetWindowPos().X + offset.X;
                if (pos.Y < ImGui.GetWindowPos().Y + offset.Y)
                    pos.Y = ImGui.GetWindowPos().Y + offset.Y;
                if (pos.X > ImGui.GetWindowPos().X + rect.Z + offset.X)
                    pos.X = ImGui.GetWindowPos().X + rect.Z + offset.X;
                if (pos.Y > ImGui.GetWindowPos().Y + rect.W + offset.Y)
                    pos.Y = ImGui.GetWindowPos().Y + rect.W + offset.Y;
                var pixel = origin.GetPixel(Convert.ToInt16(pos.X - offset.X), Convert.ToInt16(pos.Y - offset.Y));
                minrgb = new Vector3(applyTol(pixel.R,false), applyTol(pixel.G, false), applyTol(pixel.B, false));
                
                maxrgb = new Vector3(applyTol(pixel.R, true), applyTol(pixel.G, true), applyTol(pixel.B, true));
            } catch
            {

            }
            if (ImGui.IsMouseClicked(ImGuiMouseButton.Left))
            {
                state = 0;
            }
        }
        
        void drawEditor(Vector2 point,Vector2 size)
        {
            var draw = ImGui.GetWindowDrawList();
            draw.AddImage(originsrc_dark, point,size);
            var tempPoints = datapoints.OrderBy(x=>x.angle).ToArray();
            if (tempPoints.Length == 0) return;
            Vector2 pos = ImGui.GetMousePos();
            for (int i = 1;i< tempPoints.Length;i++)
            {
                Vector2 start = tempPoints[i - 1].toCartisian() + center + point;
                Vector2 end = tempPoints[i].toCartisian() + center + point;
                draw.AddLine(start,end, 0xFFFF0000);
            }
            if (ImGui.IsMouseHoveringRect(point, point + size))
            {
                Polarizer.polar polarMouse = new Polarizer.polar(pos - point - center);
                var closestPolar = tempPoints.First(x=>x.angle == Math.Round(polarMouse.angle + 180)%360);
                draw.AddText(point, 0XFF00FF00, $"mouse\n {polarMouse.angle} Degress\n {polarMouse.magnitude} mag");
                draw.AddLine(point + center, pos,0XFFFFFFFF);
                draw.AddCircleFilled(closestPolar.toCartisian() + center + point, 4,0x000000FF);
                if(ImGui.IsMouseDown(ImGuiMouseButton.Left))
                {

                    datapoints.Remove(closestPolar);
                    closestPolar.magnitude = polarMouse.magnitude;
                    datapoints.Add(closestPolar);
                }
            }
        }
        float applyTol(float val, bool positive)
        {
            
            val = val + ((positive) ? colortol:-colortol);
            if (val > 255)
                val = 255;
            if (val < 0)
                val = 0;
            return val;
        }
        void createCSVFILE()
        {
            string text = "angle,mag";
            datapoints = datapoints.OrderBy(x => x.angle).ToList();
            polar[] flip = datapoints.ToArray();
            //shifting 180 degrees
            for (int i = 0; i < 360; i++)
            {
                flip[i].angle = (Convert.ToInt32(flip[i].angle + 180)) % 360;
            }
            datapoints = flip.ToList();
            datapoints = datapoints.OrderBy(x => x.angle).ToList();
            foreach (Polarizer.polar tmep in datapoints)
            {
                var scaledAngle = tmep.angle;
                var scaledData = (tmep.magnitude / scalePX * scaleSize) + start;
                text += $"\n{tmep.angle},{scaledData}";
            }
            File.WriteAllText(Program.filename + ".csv", text);
        }
        float colortol = 20;
        List<Polarizer.polar> datapoints = new List<Polarizer.polar>();
        protected override void Render()
        {
            ImGui.Begin("Diagramer", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            if (!loadedResources)
            {
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height));
                ImGui.SetNextWindowPos(new Vector2(0,0));
                loadresources();
                loadedResources = true;
            }
            Vector4 imagerect = new Vector4();
            if(ImGui.Button("set center"))
                state = 1;
            ImGui.SameLine();
            if (ImGui.Button("set scalePX"))
                state = 2;
            ImGui.SameLine();
            
            ImGui.Text($"center ({center.X},{center.Y})");
            ImGui.SameLine();
            ImGui.Text($"scale {scalePX}");
            ImGui.SameLine();
            ImGui.BeginChild("inputs",new Vector2(200,200));
            ImGui.InputInt("set start", ref start);
            ImGui.InputInt("set scale Size", ref scaleSize);
            ImGui.EndChild();
            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + 20);
            ImGui.BeginChild("inputsclr", new Vector2(200, 400));
            if (ImGui.Button("pick color"))
                state = 3;
            ImGui.ColorPicker3("min", ref minrgb,ImGuiColorEditFlags.NoSmallPreview |
                ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoSidePreview | 
                ImGuiColorEditFlags.NoInputs);
            ImGui.ColorPicker3("max", ref maxrgb, ImGuiColorEditFlags.NoSmallPreview |
                ImGuiColorEditFlags.NoOptions | ImGuiColorEditFlags.NoSidePreview |
                ImGuiColorEditFlags.NoInputs);
            ImGui.SliderFloat("tolerance", ref colortol, 0, 100);
            ImGui.EndChild();
            var draw = ImGui.GetWindowDrawList();
            imagerect = new Vector4(offset.X,offset.Y, origin.Width,origin.Height);
            draw.AddImage(originsrc, ImGui.GetWindowPos() + offset, ImGui.GetWindowPos() + new System.Numerics.Vector2(origin.Width + offset.X, origin.Height + offset.Y));
            draw.AddCircleFilled(center + offset, 4, 0xFF00FF00);
            draw.AddText(center + offset, 0xFF000000, $"center ({center.X},{center.Y})");
            draw.AddText(new Vector2(center.X - scalePX,center.Y) + offset, 0xFF000000, $"scale {scalePX} px");
            switch (state)
            {
                case 1:
                    drawCenterPicker(imagerect);
                    break;
                    case 2:
                    drawScalePicker(imagerect);
                    break;
                case 3:
                    drawColorPicker(imagerect);
                    break;
                default:
                    break;
            }
            ImGui.SameLine();
            if(ImGui.Button("detect"))
            {
                
                (results, datapoints) = Polarizer.begin(origin, new Point(Convert.ToInt16(center.X), Convert.ToInt16(center.Y)), start, scaleSize, scalePX, 
                    new Emgu.CV.Structure.Rgb(minrgb.X, minrgb.Y, minrgb.Z), new Emgu.CV.Structure.Rgb(maxrgb.X, maxrgb.Y, maxrgb.Z));
                AddOrGetImagePointer("results", Polarizer.ConvertBitmapToImageSharp(results), true, out resultssrc);
            }
            if(results != null)
            {
                draw.AddImage(resultssrc, ImGui.GetWindowPos() + offset + new Vector2(origin.Width,0), ImGui.GetWindowPos() + new System.Numerics.Vector2(origin.Width*2 + offset.X, origin.Height + offset.Y));
                drawEditor(ImGui.GetWindowPos() + offset + new Vector2(origin.Width*2, 0), ImGui.GetWindowPos() + new System.Numerics.Vector2(origin.Width*3 + offset.X, origin.Height + offset.Y));
                if (ImGui.Button("Save Results"))
                {
                    createCSVFILE();
                }
            }
            
            ImGui.End();
            
        }
    }
}
