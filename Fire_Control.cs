using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Fire_Control : MonoBehaviour
{
    [SerializeField] HingeJoint Turret_Horizontal;
    [SerializeField] HingeJoint Turret_Vertical;
    [SerializeField] HingeJoint Shooter_Lens;
    [SerializeField] GameObject Shooter_Lens_Obj;
    [SerializeField] Transform Turret_Horizontal_Position;
    [SerializeField] Transform Turret_Vertical_Position;
    [SerializeField] Transform Shooter_Lens_Position;
    [SerializeField] Transform Platform_Position;
    private JointMotor THM;
    private JointMotor TVM;
    private JointMotor SLM;
    private JointLimits Stay_Angle;
    private float Horizontal_Rotation_Speed = 200f;
    private float Horizontal_Rotation_Force = 20f;
    private float Horizontal_Rotation_Contain_Force = 10000f;
    private float Vertical_Rotation_Speed = 100f;
    private float Vertical_Rotation_Force = 10f;
    private float Vertical_Rotation_Contain_Force = 10000f;
    private float Vertical_Rotation_Angle_Max = 40f;
    private float Vertical_Rotation_Angle_Min = -20f;
    private float Shooter_Lens_Speed = 100f;
    private float Shooter_Lens_Force = 10f;
    private float Shooter_Lens_Contain_Force = 10000f;
    private float Shooter_Lens_Angle_Max = 40f;
    private float Shooter_Lens_Angle_Min = -25f;
    private float cervice = 0.0001f;
    private float Gun_Lens_offset = 0f;
    private float inter_angle = 0f;
    

    private float Stay_Angle_Transmit = 0f;
    private float Horizontal_Angle_Transmit = 0f;
    private float Vertical_Angle_Transmit = 0f;
    private float Lens_Angle_Transmit = 0f;
    private float hold_force = 0f;
    private Vector3 Euler_Angles_Transmit;


    private float ivTHT;
    private float ivTVT;
    private float Gun_Orientation;
    private GameObject Input_TH;
    private GameObject Input_TV;
    private GameObject Input_DO;
    [SerializeField] Scrollbar ITH;
    [SerializeField] Scrollbar ITV;
    [SerializeField] Scrollbar IDO;

    private RaycastHit RayHit;
    private int lay_mask;
    public Vector3 target_position;
    private bool found_target = false;

    Quaternion horizonal_rotation;
    Matrix4x4 hrm;
    Vector3 A;
    Vector3 B;

    private Vector3 BC = new Vector3(-0.4f, 0.4f, 1.6f); 
    Vector3 C;

    private float CH;
    private float SH;
    private Vector3 CP;

    // Start is called before the first frame update
    void Start()
    {
        THM.targetVelocity = 0f;
        THM.force = Horizontal_Rotation_Contain_Force;
        THM.freeSpin = false;
        Turret_Horizontal.useMotor = true;

        TVM.targetVelocity = 0f;
        TVM.force = Vertical_Rotation_Contain_Force;
        TVM.freeSpin = false;
        Turret_Vertical.useMotor = true;

        SLM.targetVelocity = 0f;
        SLM.force = Shooter_Lens_Force;
        SLM.freeSpin = false;
        Shooter_Lens.useMotor = false;

        Stay_Angle_Transmit = Turret_Vertical_Position.transform.localEulerAngles.x;
        if(Stay_Angle_Transmit > 180f)
        {
            Stay_Angle_Transmit = Stay_Angle_Transmit - 360f;
        }
        Stay_Angle.max = Stay_Angle_Transmit + cervice;
        Stay_Angle.min = Stay_Angle_Transmit;
        Turret_Vertical.limits = Stay_Angle;

        // Stay_Angle_Transmit = Shooter_Lens_Position.transform.localEulerAngles.x;
        // if(Stay_Angle_Transmit > 180f)
        // {
        //     Stay_Angle_Transmit = Stay_Angle_Transmit - 360f;
        // }
        // Stay_Angle.max = Stay_Angle_Transmit + cervice;
        // Stay_Angle.min = Stay_Angle_Transmit;
        // Shooter_Lens.limits = Stay_Angle;

        ITH = Input_TH.GetComponent<Scrollbar>();
        ITV = Input_TV.GetComponent<Scrollbar>();
        IDO = Input_DO.GetComponent<Scrollbar>();

        lay_mask = 1<<LayerMask.NameToLayer("Default");

        // BC = new Vector3(-0.4f, 0.4f, 1.6f);
    }


    // Update is called once per frame
    private void FixedUpdate()
    {
        Gun_Lens_offset = 15f * IDO.value;
        ivTHT = (360f * ITH.value) - 180f;
        ivTVT = (60f * ITV.value) - 20f;
        
        // print("Turret_Horizontal_Terget_Value: "+ivTHT+" || Turret_Vertical_Terget_Value: "+ivTVT);

        horizonal_rotation = Quaternion.Euler(Platform_Position.localEulerAngles.x, Platform_Position.localEulerAngles.y, Platform_Position.localEulerAngles.z);
        hrm = Matrix4x4.Rotate(horizonal_rotation);
        A = target_position - Platform_Position.position;
        A = new Vector3(hrm.m00*A.x + hrm.m10*A.y + hrm.m20*A.z, hrm.m01*A.x + hrm.m11*A.y + hrm.m21*A.z, hrm.m02*A.x + hrm.m12*A.y + hrm.m22*A.z);
        // B = Shooter_Lens_Position.position - Platform_Position.position;
        // B = new Vector3(hrm.m00*B.x + hrm.m10*B.y + hrm.m20*B.z, hrm.m01*B.x + hrm.m11*B.y + hrm.m21*B.z, hrm.m02*B.x + hrm.m12*B.y + hrm.m22*B.z);
        
        

        // Debug.DrawRay(Platform_Position.position, Shooter_Lens_Position.position - Platform_Position.position, Color.blue);

        if(Physics.Raycast(Shooter_Lens_Obj.GetComponent<Transform>().position, Shooter_Lens_Obj.GetComponent<Transform>().up, out RayHit))
        {
            Debug.DrawRay(Shooter_Lens_Obj.GetComponent<Transform>().position, RayHit.point - Shooter_Lens_Obj.GetComponent<Transform>().position, Color.green);
        }
        else
        {
            Debug.DrawRay(Shooter_Lens_Obj.GetComponent<Transform>().position, Shooter_Lens_Obj.GetComponent<Transform>().up, Color.red);
        }
        // if(Input.GetKey(KeyCode.LeftArrow))
        // {
        //     THM.targetVelocity = -1 * Horizontal_Rotation_Speed;
        //     THM.force = Horizontal_Rotation_Force;
        // }
        // else if(Input.GetKey(KeyCode.RightArrow))
        // {
        //     THM.targetVelocity = Horizontal_Rotation_Speed;
        //     THM.force = Horizontal_Rotation_Force;
        // }
        // else
        // {
        //     THM.targetVelocity = 0f;
        //     THM.force = Horizontal_Rotation_Contain_Force;
        // }

        if(Input.GetKeyDown(KeyCode.X))
        {
            found_target = false;
        }

        if(Input.GetKeyDown(KeyCode.F))
        {
            if(Physics.Raycast(Shooter_Lens_Obj.GetComponent<Transform>().position, Shooter_Lens_Obj.GetComponent<Transform>().up, out RayHit))
            {
                target_position = RayHit.point;
                found_target = true;
                // Debug.Log(A+", "+B);
                // Debug.Log(Mathf.Asin(0.5f) * Mathf.Rad2Deg);
            }
            else
            {
                found_target = false;
                Debug.Log("No Target Captured");
            }
        }

        if(found_target)
        {
            Debug.DrawRay(Platform_Position.position, target_position - Platform_Position.position, Color.blue);

            // Vector3 v = new Vector3(Mathf.Cos(ivTHT*Mathf.Deg2Rad), Mathf.Sin(ivTHT*Mathf.Deg2Rad),0);
            // v = new Vector3(hrm.m00*v.x + hrm.m01*v.y + hrm.m02*v.z, hrm.m10*v.x + hrm.m11*v.y + hrm.m12*v.z, hrm.m20*v.x + hrm.m21*v.y + hrm.m22*v.z);
            // Debug.DrawRay(Platform_Position.position, v*5, Color.magenta);

            // Vector3 k = new Vector3(BC.x*Mathf.Cos(ivTHT*Mathf.Deg2Rad)-BC.y*Mathf.Sin(ivTHT*Mathf.Deg2Rad), BC.x*Mathf.Sin(ivTHT*Mathf.Deg2Rad)+BC.y*Mathf.Cos(ivTHT*Mathf.Deg2Rad), BC.z);
            // k = new Vector3(hrm.m00*k.x + hrm.m01*k.y + hrm.m02*k.z, hrm.m10*k.x + hrm.m11*k.y + hrm.m12*k.z, hrm.m20*k.x + hrm.m21*k.y + hrm.m22*k.z);
            // Debug.DrawRay(Platform_Position.position, k*2, Color.cyan);

             
            ivTHT = Mathf.Asin(BC.x / Mathf.Sqrt(A.x * A.x + A.y * A.y)) * Mathf.Rad2Deg - Mathf.Asin(A.x / Mathf.Sqrt(A.x * A.x + A.y * A.y)) * Mathf.Rad2Deg;
            CH = Mathf.Cos(ivTHT*Mathf.Deg2Rad);
            SH = Mathf.Sin(ivTHT*Mathf.Deg2Rad);
            ivTVT = Vector3.Angle(new Vector3(A.x - BC.x*CH + BC.y*SH, A.y - BC.x*SH - BC.y*CH, A.z - BC.z),new Vector3(-1*SH, CH,0));
            CP = Vector3.Cross(new Vector3(-1*SH, CH,0), new Vector3(A.x - BC.x*CH + BC.y*SH, A.y - BC.x*SH - BC.y*CH, A.z - BC.z));
            // Debug.Log();
            if((CH*CP.x + SH*CP.y) < 0)
            {
                ivTVT = -1 * ivTVT;
            }
            // Vector3 j = new Vector3(CH*CP.x + SH*CP.y, -1*SH*CP.x + CH*CP.y, CP.z);
            // j = new Vector3(hrm.m00*j.x + hrm.m01*j.y + hrm.m02*j.z, hrm.m10*j.x + hrm.m11*j.y + hrm.m12*j.z, hrm.m20*j.x + hrm.m21*j.y + hrm.m22*j.z);
            // Debug.DrawRay(Platform_Position.position, j*2, Color.cyan);
        }

        if(ivTVT > (Vertical_Rotation_Angle_Max - Gun_Lens_offset))
        {
            ivTVT = Vertical_Rotation_Angle_Max - Gun_Lens_offset;
            ITV.value = (ivTVT + 20f) / 60f;
        }
        Gun_Orientation = ivTVT + Gun_Lens_offset;



        Horizontal_Angle_Transmit = Turret_Horizontal_Position.transform.localEulerAngles.z;
        if(Horizontal_Angle_Transmit > 180f)
        {
            Horizontal_Angle_Transmit = Horizontal_Angle_Transmit - 360f;
        }

        if(ivTHT > Horizontal_Angle_Transmit)
        {
            if((ivTHT - Horizontal_Angle_Transmit) < 180f)
            {
                if((ivTHT - Horizontal_Angle_Transmit) > 10f)
                {
                    THM.targetVelocity = Horizontal_Rotation_Speed;
                    THM.force = Horizontal_Rotation_Force;
                }
                else
                {
                    THM.targetVelocity = Horizontal_Rotation_Speed * (ivTHT - Horizontal_Angle_Transmit) / 10f;
                    hold_force = Horizontal_Rotation_Force * 10f / (ivTHT - Horizontal_Angle_Transmit);
                    THM.force = (hold_force < Horizontal_Rotation_Contain_Force)? hold_force : Horizontal_Rotation_Contain_Force;
                }
            }
            else
            {
                if((Horizontal_Angle_Transmit - (ivTHT - 360f)) > 10f)
                {
                    THM.targetVelocity = -1 * Horizontal_Rotation_Speed;
                    THM.force = Horizontal_Rotation_Force;
                }
                else
                {
                    THM.targetVelocity = Horizontal_Rotation_Speed * ((ivTHT - 360f) - Horizontal_Angle_Transmit) / 10f;
                    hold_force = Horizontal_Rotation_Force * 10f / (Horizontal_Angle_Transmit - (ivTHT - 360f));
                    THM.force = (hold_force < Horizontal_Rotation_Contain_Force)? hold_force : Horizontal_Rotation_Contain_Force;
                }
            }
            
            
        }
        else if(ivTHT < Horizontal_Angle_Transmit)
        {
            if(Horizontal_Angle_Transmit - ivTHT < 180f)
            {
                if((Horizontal_Angle_Transmit - ivTHT) > 10f)
                {
                    THM.targetVelocity = -1 * Horizontal_Rotation_Speed;
                    THM.force = Horizontal_Rotation_Force;
                }
                else
                {
                    THM.targetVelocity = Horizontal_Rotation_Speed * (ivTHT - Horizontal_Angle_Transmit) / 10f;
                    hold_force = Horizontal_Rotation_Force * 10f / (Horizontal_Angle_Transmit - ivTHT);
                    THM.force = (hold_force < Horizontal_Rotation_Contain_Force)? hold_force : Horizontal_Rotation_Contain_Force;
                }
            }
            else
            {
                if(((ivTHT + 360f) - Horizontal_Angle_Transmit) > 10f)
                {
                    THM.targetVelocity = Horizontal_Rotation_Speed;
                    THM.force = Horizontal_Rotation_Force;
                }
                else
                {
                    THM.targetVelocity = Horizontal_Rotation_Speed * ((ivTHT + 360f) - Horizontal_Angle_Transmit) / 10f;
                    hold_force = Horizontal_Rotation_Force * 10f / ((ivTHT + 360f) - Horizontal_Angle_Transmit);
                    THM.force = (hold_force < Horizontal_Rotation_Contain_Force)? hold_force : Horizontal_Rotation_Contain_Force;
                }
            }
            
        }
        else
        {
            THM.targetVelocity = 0f;
            THM.force = Horizontal_Rotation_Contain_Force;
        }


        Vertical_Angle_Transmit = Turret_Vertical_Position.transform.localEulerAngles.x;
        if(Vertical_Angle_Transmit > 180f)
        {
            Vertical_Angle_Transmit = Vertical_Angle_Transmit - 360f;
        }

        if(Gun_Orientation > Vertical_Angle_Transmit)
        {
            Stay_Angle.max = Vertical_Rotation_Angle_Max;
            Stay_Angle.min = Vertical_Rotation_Angle_Min;
            Turret_Vertical.limits = Stay_Angle;

            if((Gun_Orientation - Vertical_Angle_Transmit) > 1f)
            {
                TVM.targetVelocity = Vertical_Rotation_Speed;
                TVM.force = Vertical_Rotation_Force;
            }
            else
            {
                TVM.targetVelocity = Vertical_Rotation_Speed * (Gun_Orientation - Vertical_Angle_Transmit) / 1f;
                hold_force = Vertical_Rotation_Force * 1f / (Gun_Orientation - Vertical_Angle_Transmit);
                TVM.force = (hold_force < Vertical_Rotation_Contain_Force)? hold_force : Vertical_Rotation_Contain_Force;
            }

        }
        else if(Gun_Orientation < Vertical_Angle_Transmit)
        {
            Stay_Angle.max = Vertical_Rotation_Angle_Max;
            Stay_Angle.min = Vertical_Rotation_Angle_Min;
            Turret_Vertical.limits = Stay_Angle;

            if((Vertical_Angle_Transmit - Gun_Orientation) > 1f)
            {
                TVM.targetVelocity = -1 * Vertical_Rotation_Speed;
                TVM.force = Vertical_Rotation_Force;
            }
            else
            {
                TVM.targetVelocity = Vertical_Rotation_Speed * (Gun_Orientation - Vertical_Angle_Transmit) / 1f;
                hold_force = Vertical_Rotation_Force * 1f / (Vertical_Angle_Transmit - Gun_Orientation);
                TVM.force = (hold_force < Vertical_Rotation_Contain_Force)? hold_force : Vertical_Rotation_Contain_Force;
            }
        }
        else
        {
            Stay_Angle_Transmit = Turret_Vertical_Position.transform.localEulerAngles.x;
            if(Stay_Angle_Transmit > 180f)
            {
                Stay_Angle_Transmit = Stay_Angle_Transmit - 360f;
            }
            Stay_Angle.max = Stay_Angle_Transmit + cervice;
            Stay_Angle.min = Stay_Angle_Transmit;
            Turret_Vertical.limits = Stay_Angle;

            TVM.targetVelocity = 0f;
            TVM.force = Vertical_Rotation_Contain_Force;
        }


        Euler_Angles_Transmit = Shooter_Lens_Position.transform.localEulerAngles; 
        Lens_Angle_Transmit = Shooter_Lens_Position.transform.localEulerAngles.x;
        if(Lens_Angle_Transmit > 180f)
        {
            Lens_Angle_Transmit = Lens_Angle_Transmit -360f;
        }
        inter_angle = Vertical_Angle_Transmit - Lens_Angle_Transmit - Gun_Lens_offset;
        if(inter_angle >= -0.2f && inter_angle <= 0.2f)
        {
            // print("follow: " + Vertical_Angle_Transmit + " Lens: " + Lens_Angle_Transmit + " Offset: " + Gun_Lens_offset);
            if((Vertical_Angle_Transmit - Gun_Lens_offset) > Shooter_Lens.limits.min)
            {
                Euler_Angles_Transmit.x = Vertical_Angle_Transmit - Gun_Lens_offset;
            }
            else
            {
                Euler_Angles_Transmit.x = Shooter_Lens.limits.min;
            }
        }
        else
        {
            // print("seperate| Gun: " + Vertical_Angle_Transmit + " Lens: " + Lens_Angle_Transmit + " Offset: " + Gun_Lens_offset);
            if(Gun_Orientation > Shooter_Lens.limits.min)
            {
                Euler_Angles_Transmit.x = ((Gun_Orientation < Vertical_Rotation_Angle_Max)? Gun_Orientation : Vertical_Rotation_Angle_Max) - Gun_Lens_offset;
            }
            else
            {
                Euler_Angles_Transmit.x = Shooter_Lens.limits.min;
            }
        }
        Shooter_Lens_Position.transform.localEulerAngles = Euler_Angles_Transmit;
        
        // print(Vertical_Angle_Transmit + " || " + Euler_Angles_Transmit.x);
        // IDO.value = (Turret_Vertical_Position.transform.localEulerAngles.x - Euler_Angles_Transmit.x) / 15f;
        
        
        // if(Input.GetKey(KeyCode.UpArrow))
        // {
        //     Stay_Angle.max = Vertical_Rotation_Angle_Max;
        //     Stay_Angle.min = Vertical_Rotation_Angle_Min;
        //     Turret_Vertical.limits = Stay_Angle;

        //     TVM.targetVelocity = Vertical_Rotation_Speed;
        //     TVM.force = Vertical_Rotation_Force;


        //     Stay_Angle.max = Shooter_Lens_Angle_Max;
        //     Stay_Angle.min = Shooter_Lens_Angle_Min;
        //     Shooter_Lens.limits = Stay_Angle;

        //     SLM.targetVelocity = Shooter_Lens_Speed;
        //     SLM.force = Shooter_Lens_Speed;
        // }
        // else if(Input.GetKey(KeyCode.DownArrow))
        // {
        //     Stay_Angle.max = Vertical_Rotation_Angle_Max;
        //     Stay_Angle.min = Vertical_Rotation_Angle_Min;
        //     Turret_Vertical.limits = Stay_Angle;

        //     TVM.targetVelocity = -1 * Vertical_Rotation_Speed;
        //     TVM.force = Vertical_Rotation_Force;


        //     Stay_Angle.max = Shooter_Lens_Angle_Max;
        //     Stay_Angle.min = Shooter_Lens_Angle_Min;
        //     Shooter_Lens.limits = Stay_Angle;
            
        //     SLM.targetVelocity = -1 * Shooter_Lens_Speed;
        //     SLM.force = Shooter_Lens_Speed;
        // }
        // else if(Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.DownArrow))
        // {
        //     Stay_Angle_Transmit = Turret_Vertical_Position.transform.localEulerAngles.x;
        //     if(Stay_Angle_Transmit > 180f)
        //     {
        //         Stay_Angle_Transmit = Stay_Angle_Transmit - 360f;
        //     }
        //     Stay_Angle.max = Stay_Angle_Transmit + cervice;
        //     Stay_Angle.min = Stay_Angle_Transmit;
        //     Turret_Vertical.limits = Stay_Angle;

        //     TVM.targetVelocity = 0f;
        //     TVM.force = Vertical_Rotation_Contain_Force;


        //     Stay_Angle_Transmit = Shooter_Lens_Position.transform.localEulerAngles.x;
        //     if(Stay_Angle_Transmit > 180f)
        //     {
        //         Stay_Angle_Transmit = Stay_Angle_Transmit - 360f;
        //     }
        //     Stay_Angle.max = Stay_Angle_Transmit + cervice;
        //     Stay_Angle.min = Stay_Angle_Transmit;

        //     Shooter_Lens.limits = Stay_Angle;
        //     SLM.targetVelocity = 0f;
        //     SLM.force = Shooter_Lens_Contain_Force;
        // }
        // else
        // {        
        //     TVM.targetVelocity = 0f;
        //     TVM.force = Vertical_Rotation_Contain_Force;

        //     SLM.targetVelocity = 0f;
        //     SLM.force = Shooter_Lens_Contain_Force;
        // }

        Turret_Horizontal.motor = THM;
        Turret_Vertical.motor = TVM;
        // Shooter_Lens.motor = SLM;
        // variation_offset = Gun_Lens_offset;
    }

}
