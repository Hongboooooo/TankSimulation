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
    [SerializeField] GameObject Observer_set;
    [SerializeField] GameObject Observer_lens;
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
    
    private float Observer_Horizonal_Speed_Times = 1f;
    private float Observer_Vertical_Speed_Times = 1f;
    private float OHKeep = 0;
    private float OVKeep = 0;

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
    private RaycastHit ObserveHit;
    private int lay_mask;
    public Vector3 target_position;
    public Vector3 observe_position;
    private bool found_target = false;
    private bool has_observation = false;

    Quaternion platform_rotation;
    Matrix4x4 hrm;
    Vector3 A;
    Vector3 B;

    Quaternion turret_rotation;
    Matrix4x4 trm;
    Vector3 D;

    private Vector3 BC = new Vector3(-0.4f, 0.4f, 1.6f); 
    Vector3 C;

    private float CH;
    private float SH;
    private Vector3 CP;

    private float iter_time = 0;
    private int ii = 0;

    [SerializeField] MouseControl MC;

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

        // MC = GameObject.Find("MouseControl").GetComponent<MouseControl>();

        // BC = new Vector3(-0.4f, 0.4f, 1.6f);
    }


    // Update is called once per frame
    private void FixedUpdate()
    {

        iter_time += Time.deltaTime;
        if(iter_time >= 0.01f)
        {
            Observer_set.GetComponent<Transform>().localEulerAngles += new Vector3(0,0,MC.move_speed.x * Observer_Horizonal_Speed_Times / 100f);
            Observer_lens.GetComponent<Transform>().localEulerAngles += new Vector3(MC.move_speed.y * Observer_Vertical_Speed_Times / 100f,0,0);
            // Debug.Log(Observer_lens.GetComponent<Transform>().localEulerAngles.x);
            if(Observer_lens.GetComponent<Transform>().localEulerAngles.x >= 180)
            {
                if(Observer_lens.GetComponent<Transform>().localEulerAngles.x < 330)
                {
                    Debug.Log("out");
                    Observer_lens.GetComponent<Transform>().localEulerAngles = new Vector3(330,Observer_lens.GetComponent<Transform>().localEulerAngles.y,Observer_lens.GetComponent<Transform>().localEulerAngles.z);
                }
            }
            else
            {
                if(Observer_lens.GetComponent<Transform>().localEulerAngles.x > 75)
                {
                    Debug.Log("out");
                    Observer_lens.GetComponent<Transform>().localEulerAngles = new Vector3(75,Observer_lens.GetComponent<Transform>().localEulerAngles.y,Observer_lens.GetComponent<Transform>().localEulerAngles.z);
                }
            }
            

            iter_time = 0;
        }

        platform_rotation = Quaternion.Euler(Platform_Position.localEulerAngles.x, Platform_Position.localEulerAngles.y, Platform_Position.localEulerAngles.z);
        hrm = Matrix4x4.Rotate(platform_rotation);

        // Debug.Log(has_observation);
        // if(MC.get_input)
        // {
        //     if(Physics.Raycast(Observer_lens.GetComponent<Transform>().position, Observer_lens.GetComponent<Transform>().up, out RayHit))
        //     {
        //         observe_position = ObserveHit.point;
        //     }
        // }

        // Debug.Log(observe_position);
        Gun_Lens_offset = 15f * IDO.value;
        ivTHT = (360f * ITH.value) - 180f;
        ivTVT = (60f * ITV.value) - 20f;
        
        // print("Turret_Horizontal_Terget_Value: "+ivTHT+" || Turret_Vertical_Terget_Value: "+ivTVT);
        
        if(Physics.Raycast(Observer_lens.GetComponent<Transform>().position, Observer_lens.GetComponent<Transform>().up, out ObserveHit))
        {
            if(has_observation)
            {
                Debug.DrawRay(Observer_lens.GetComponent<Transform>().position, ObserveHit.point - Observer_lens.GetComponent<Transform>().position, Color.blue);
            }
            else
            {
                Debug.DrawRay(Observer_lens.GetComponent<Transform>().position, ObserveHit.point - Observer_lens.GetComponent<Transform>().position, Color.cyan);
            }
            
        }
        else
        {
            Debug.DrawRay(Observer_lens.GetComponent<Transform>().position, Observer_lens.GetComponent<Transform>().up, Color.gray);
        }

        // Debug.DrawRay(Platform_Position.position, Shooter_Lens_Position.position - Platform_Position.position, Color.blue);
        // Debug.DrawRay(Platform_Position.position, Observer_lens.GetComponent<Transform>().position - Platform_Position.position, Color.blue);

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

        if(Input.GetKeyDown(KeyCode.F))
        {
            if(Physics.Raycast(Observer_lens.GetComponent<Transform>().position, Observer_lens.GetComponent<Transform>().up, out RayHit))
            {
                target_position = ObserveHit.point;
                observe_position = ObserveHit.point;
                found_target = true;
                has_observation = true;
            }
            else
            {
                found_target = false;
                has_observation = false;
                Debug.Log("No Target Captured");
            }
            
        }

        if(Input.GetKeyDown(KeyCode.X))
        {
            found_target = false;
            has_observation = false;
        }

        if(MC.get_input)
        {
            has_observation = false;
        }

        if(found_target)
        {
            // Debug.DrawRay(Platform_Position.position, target_position - Platform_Position.position, Color.blue);
  
            A = target_position - Platform_Position.position;
            A = new Vector3(hrm.m00*A.x + hrm.m10*A.y + hrm.m20*A.z, hrm.m01*A.x + hrm.m11*A.y + hrm.m21*A.z, hrm.m02*A.x + hrm.m12*A.y + hrm.m22*A.z);

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

        }

        if(has_observation)
        {
            turret_rotation = Quaternion.Euler(Turret_Horizontal_Position.localEulerAngles.x, Turret_Horizontal_Position.localEulerAngles.y, Turret_Horizontal_Position.localEulerAngles.z);
            trm = Matrix4x4.Rotate(turret_rotation);
            trm = hrm * trm;

            D = observe_position - Observer_lens.GetComponent<Transform>().position;
            D = new Vector3(trm.m00*D.x + trm.m10*D.y + trm.m20*D.z, trm.m01*D.x + trm.m11*D.y + trm.m21*D.z, trm.m02*D.x + trm.m12*D.y + trm.m22*D.z); //global view to local view

            OHKeep = Mathf.Atan(-1*D.x/D.y);
            Observer_set.GetComponent<Transform>().localEulerAngles = new Vector3(0,0,OHKeep*Mathf.Rad2Deg);
            OVKeep = Vector3.Angle(new Vector3(-1*Mathf.Sin(OHKeep),Mathf.Cos(OHKeep),0),D);
            CH = Mathf.Cos(OHKeep);
            SH = Mathf.Sin(OHKeep);
            CP = Vector3.Cross(new Vector3(-1*SH, CH,0), D);
            if((CH*CP.x + SH*CP.y) < 0)
            {
                OVKeep = -1 * OVKeep;
            }
            Observer_lens.GetComponent<Transform>().localEulerAngles = new Vector3(OVKeep,0,0);
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
