using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HL_AR_Dock
{
    /// <summary>
    /// This was a test to see what directions lines were drawn
    /// </summary>
    public class AR_Dock_Lines : PartModule
    {
        public LineRenderer Line_Dock, Line_Velocity; // Line_Forward, Line_Up, Line_Right, 
        public LineRenderer Line_V_Up, Line_V_Right, Line_V_Diff; // Line_V_Down, Line_V_Left,

        public Color Color_Dock = Color.white, Color_Velocity = Color.yellow, Color_Diff = Color.blue;

        [KSPField(guiActive = true, guiActiveEditor = true, guiName = "Active", isPersistant = true)]
        bool toggle = true;

        [KSPEvent(active = true, guiActive = true, guiActiveEditor = true, guiName = "Toogle Active")]
        public void actTg(KSPActionParam kap)
        {
            toggle = !toggle;
        }

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Vessel Is Active")]
        bool vIsActive = false;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Found Target")]
        bool foundTarget = false;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Target Loaded")]
        bool loadedTarget = false;

        [KSPField(guiActive = true, guiActiveEditor = false, guiName = "Alignment", guiFormat = "F0")]
        float alignment = 0f;

        private Vector3 velocity_smoothed;
        private Vector3 arrow_point, arrow_to_point;
        private ITargetable vTarget;

        public override void OnStart(StartState state)
        {
            /*
            GameObject g1 = new GameObject("g1");
            g1.transform.parent = this.transform; g1.transform.localPosition = Vector3.zero;
            Line_Forward = g1.AddComponent<LineRenderer>();
            makeLineRenderers(Line_Forward, Color.blue);

            GameObject g2 = new GameObject("g2");
            g2.transform.parent = this.transform; g2.transform.localPosition = Vector3.zero;
            Line_Up = g2.AddComponent<LineRenderer>();
            makeLineRenderers(Line_Up, Color.green);

            GameObject g3 = new GameObject("g3");
            g3.transform.parent = this.transform; g3.transform.localPosition = Vector3.zero;
            Line_Right = g3.AddComponent<LineRenderer>();
            makeLineRenderers(Line_Right, Color.red);
            */

            GameObject g4 = new GameObject("g4");
            g4.transform.parent = this.transform; g4.transform.localPosition = Vector3.zero;
            Line_Dock = g4.AddComponent<LineRenderer>();
            makeLineRenderers(Line_Dock, 0.4f, 0.0f);
            setLineColors(Line_Dock, Color_Dock);

            GameObject g5 = new GameObject("g5");
            g5.transform.parent = this.transform; g5.transform.localPosition = Vector3.zero;
            Line_Velocity = g5.AddComponent<LineRenderer>();
            makeLineRenderers(Line_Velocity, 0.4f, 0.2f);
            setLineColors(Line_Velocity, Color_Velocity);

            GameObject g7 = new GameObject("g7");
            g7.transform.parent = this.transform; g7.transform.localPosition = Vector3.zero;
            Line_V_Up = g7.AddComponent<LineRenderer>();
            makeLineRenderers(Line_V_Up, 0.2f, 0.0f);
            setLineColors(Line_V_Up, Color_Velocity);

            GameObject g9 = new GameObject("g9");
            g9.transform.parent = this.transform; g9.transform.localPosition = Vector3.zero;
            Line_V_Right = g9.AddComponent<LineRenderer>();
            makeLineRenderers(Line_V_Right, 0.2f, 0.0f);
            setLineColors(Line_V_Right, Color_Velocity);

            GameObject g10 = new GameObject("g10");
            g10.transform.parent = this.transform; g10.transform.localPosition = Vector3.zero;
            Line_V_Diff = g10.AddComponent<LineRenderer>();
            makeLineRenderers(Line_V_Diff, 0.2f, 0.0f);
            setLineColors(Line_V_Diff, Color_Diff);

            /*
            GameObject g6 = new GameObject("g6");
            g6.transform.parent = this.transform; g6.transform.localPosition = Vector3.zero;
            Line_V_Down = gameObject.AddComponent<LineRenderer>();
            makeLineRenderers(Line_V_Down, 0.2f, 0.0f);
            setLineColors(Line_V_Down, Color_Velocity);

            GameObject g8 = new GameObject("g8");
            g8.transform.parent = this.transform; g8.transform.localPosition = Vector3.zero;
            Line_V_Left = gameObject.AddComponent<LineRenderer>();
            makeLineRenderers(Line_V_Left, 0.2f, 0.0f);
            setLineColors(Line_V_Left, Color_Velocity);
            */
        }

        private void makeLineRenderers(LineRenderer lR, float start, float end)
        {
            lR.material = new Material(Shader.Find("Legacy Shaders/Particles/Additive"));
            lR.SetPosition(0, Vector3.zero);
            lR.SetPosition(1, Vector3.zero);
            lR.startWidth = start;
            lR.endWidth = end;
            //lR.SetWidth(start, end);
        }

        private void setLineColors(LineRenderer lR, Color c)
        {
            Color cFade = new Color(c.r, c.g, c.b, 0f);
            lR.startColor = cFade;
            lR.endColor = c;
            //lR.SetColors(cFade, c);
        }

        public override void OnUpdate()
        {
            /*
            Line_Forward.SetPosition(0, transform.position); Line_Forward.SetPosition(1, transform.position + vessel.vesselTransform.forward);
            Line_Up.SetPosition(0, transform.position); Line_Up.SetPosition(1, transform.position + vessel.vesselTransform.up);
            Line_Right.SetPosition(0, transform.position); Line_Right.SetPosition(1, transform.position + vessel.vesselTransform.right);
            */

            vTarget = FlightGlobals.fetch.VesselTarget;
            vIsActive = vessel.isActiveVessel;
            foundTarget = vTarget != null;
            loadedTarget = foundTarget && vTarget.GetVessel() != null && vTarget.GetVessel().loaded;


            if (!toggle || !vIsActive || !foundTarget || !loadedTarget)
            {
                Line_Dock.SetPosition(0, Vector3.zero); Line_Dock.SetPosition(1, Vector3.zero);
                Line_Velocity.SetPosition(0, Vector3.zero); Line_Velocity.SetPosition(1, Vector3.zero);

                Line_V_Up.SetPosition(0, Vector3.zero); Line_V_Right.SetPosition(0, Vector3.zero); Line_V_Diff.SetPosition(0, Vector3.zero);
                Line_V_Up.SetPosition(1, Vector3.zero); Line_V_Right.SetPosition(1, Vector3.zero); Line_V_Diff.SetPosition(1, Vector3.zero);
                alignment = 0f;
                return;
            }

            velocity_smoothed = (velocity_smoothed * 0.9f + (vessel.rootPart.Rigidbody.velocity - vTarget.GetVessel().rootPart.Rigidbody.velocity) * Time.deltaTime * 0.1f) / 10f; // Sliding window

            arrow_point = vessel.GetTransform().position + velocity_smoothed.normalized;

            alignment = 100f * Vector3.Dot(velocity_smoothed.normalized, (vTarget.GetTransform().transform.position - vessel.GetTransform().position).normalized);
            if (alignment > 99.9f)
            {
                setLineColors(Line_Dock, Color_Velocity);
                setLineColors(Line_V_Diff, Color_Velocity);
                arrow_to_point = arrow_point;
            }
            else
            {
                setLineColors(Line_Dock, Color_Dock);
                setLineColors(Line_V_Diff, Color_Diff);
                arrow_to_point = Vector3.ProjectOnPlane(((vTarget.GetTransform().transform.position - vessel.GetTransform().position).normalized - arrow_point + vessel.GetTransform().position), vessel.GetTransform().up).normalized + arrow_point;
            }

            Line_Dock.SetPosition(0, vessel.GetTransform().position); // GetTransform returns the point being controlled from
            Line_Dock.SetPosition(1, vTarget.GetTransform().position); // This is the other dock port
            Line_Velocity.SetPosition(0, vessel.GetTransform().position);
            if (velocity_smoothed.magnitude > 0.00001f)
            {
                Line_Velocity.SetPosition(1, arrow_point);
                // Line_V_Down.SetPosition(0, arrow_point); Line_V_Down.SetPosition(1, arrow_point + vessel.GetTransform().up * -1f);
                Line_V_Up.SetPosition(1, arrow_point + vessel.GetTransform().forward * -1f); // Weird right? "Up" is actually "Back" on a Kerbin ship
                // Line_V_Left.SetPosition(0, arrow_point); Line_V_Left.SetPosition(1, arrow_point + vessel.GetTransform().right * -1f);
                Line_V_Right.SetPosition(1, arrow_point + vessel.GetTransform().right);
                Line_V_Diff.SetPosition(1, arrow_to_point);
            }
            else
            {
                Line_Velocity.SetPosition(1, vessel.GetTransform().position);
                Line_V_Up.SetPosition(1, arrow_point); Line_V_Right.SetPosition(1, arrow_point); Line_V_Diff.SetPosition(1, arrow_point);
            }
            Line_V_Up.SetPosition(0, arrow_point);
            Line_V_Right.SetPosition(0, arrow_point);
            Line_V_Diff.SetPosition(0, arrow_point);
            // print("Velocity is " + part.Rigidbody.velocity * Time.deltaTime);
        }
    }
}
