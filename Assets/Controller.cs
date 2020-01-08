using UnityEngine;
using System.Collections;
using DG.Tweening;

public class Controller : MonoBehaviour {
    //音频相关
    public AudioSource thisAudioSource;
    private float[] spectrumData = new float[8192];
    //cube相关
	public GameObject cubePrototype;
	public Transform startPoint;
	private Transform[] cube_transforms=new Transform[8192];
    private Vector3[] cubes_position= new Vector3[8192];
    //颜色相关
    public GridOverlay gridOverlay;
    private MeshRenderer[] cube_meshRenderers = new MeshRenderer[8192];
    private bool cubeColorChange;
    private bool gridColorChange;
    //相机移动相关
    public Vector3 cameraStartPoint;
    public Transform cameraTransform;
    public bool lookat0_1;
    public bool lookat1_2;
    public bool lookat2_3;
    public Vector3 lookat0_1_vector = Vector3.zero;
    public Vector3 lookat1_2_vector = new Vector3(106f, 12f, 78f);
    public Vector3 lookat2_3_vector = Vector3.zero;
    private Vector3[] moveTos = new Vector3[8192];
    public Transform cubes_parent;
    private bool cubesRotate = true;
	// Use this for initialization
	void Start () {
        //cube生成与排列
		Vector3 p=startPoint.position;

		for(int i=0;i<8192;i++){
			p=new Vector3(p.x+0.11f,p.y,p.z);
            GameObject cube=Object.Instantiate(cubePrototype,p,cubePrototype.transform.rotation)as GameObject;
			cube_transforms[i]=cube.transform;
            cube_meshRenderers[i] =cube.GetComponent<MeshRenderer>();
		}

		p=startPoint.position;

		float a=2f*Mathf.PI/5461;

		for(int i=0;i<5461;i++){
			cube_transforms[i].position=new Vector3(p.x+Mathf.Cos(a)*131,p.y,p.z+131*Mathf.Sin(a));
			a+=2f*Mathf.PI/5461;
            cubes_position[i]=cube_transforms[i].position;
			cube_transforms[i].parent=startPoint;
		}
        //颜色相关
        gridColorChange = false;
        cubeColorChange = false;
        Invoke("SwitchCC", 3f);
        //相机移动相关
        cameraStartPoint = cameraTransform.position;
        StartCoroutine(CameraMovement());
        //延迟播放音频
        thisAudioSource.PlayDelayed(2f);
	}
	// Update is called once per frame
	void Update () {
        Spectrum2Cube();
        DynamicColor();
        CameraLookAt();
	}
	//颜色相关
    void SwitchCC(){
        cubeColorChange = !cubeColorChange;
    }
    void SwitchGC(){
        gridColorChange = !gridColorChange;
    }
	void DynamicColor(){
        if (cubeColorChange)
        {
            for (int i = 0; i < 5461; i++)
            {
                cube_meshRenderers[i].material.SetColor("_Color", new Vector4(Mathf.Lerp(cube_meshRenderers[i].material.color.r, spectrumData[i] * 500f, 0.2f), 0.5f, 1f, 1f));
            }
        }
        if (gridColorChange)
        {
            float gridColor = Mathf.Lerp(gridOverlay.mainColor.r, spectrumData[2000] * 1000, 0.5f);
            if (gridColor > 1)
            {
                gridColor = 1;
            }
            gridOverlay.mainColor = new Vector4(gridColor, 0.5f, 1f, 1f);
        }
    }
    //thisAudioSource当前帧频率波功率，传到对应cube的localScale
    void Spectrum2Cube(){
        thisAudioSource.GetSpectrumData(spectrumData, 0, FFTWindow.BlackmanHarris);
        for (int i = 0; i < 5461; i++)
        {
            cube_transforms[i].localScale = new Vector3(0.15f, Mathf.Lerp(cube_transforms[i].localScale.y, spectrumData[i] * 10000f, 0.5f), 0.15f);
        }
    }
    //相机角度控制
    void CameraLookAt(){
        if (lookat0_1)
        {
            cameraTransform.LookAt(lookat0_1_vector);
        }
        if (lookat1_2)
        {
            cameraTransform.LookAt(lookat1_2_vector);

        }
        if (lookat2_3)
        {
            cameraTransform.LookAt(cubes_position[5190]);
        }
    }
    //网格动画
    IEnumerator GridOff()
    {
        for (int i = 0; i < 51; i++)
        {
            gridOverlay.largeStep += 10;
            yield return new WaitForSeconds(0.02f);
        }
        gridOverlay.showMain = false;

    }
    IEnumerator GridOn()
    {
        gridOverlay.showMain = true;
        gridColorChange = true;
        gridOverlay.largeStep = 500;
        for (int i = 0; i < 49; i++)
        {
            gridOverlay.largeStep -= 10;
            yield return new WaitForSeconds(0.02f);
        }
    }
    //相机重复移动，暂无退出机制
    public void CameraRepeatMove()
    {
        StopAllCoroutines();
        StartCoroutine(CameraMovement());
        if (cubesRotate)
        {
            cubesRotate = false;
            cubes_parent.DORotate(new Vector3(0f, 360f, 0f), 117f, RotateMode.FastBeyond360);
        }
        gridColorChange = false;
    }
    //相机移动脚本
    IEnumerator CameraMovement()
    {
        yield return new WaitForSeconds(20f);
        lookat2_3_vector = new Vector3(cubes_position[5200].x, 12f, cubes_position[5200].z);
        cameraTransform.DOMove(startPoint.position, 20f);
        for (int i = 0; i < 8192; i++)
        {
            moveTos[i] = new Vector3(cubes_position[i].x, 10f, cubes_position[i].z);
        }
        yield return new WaitForSeconds(20f);
        cameraTransform.DOMove(new Vector3(126f, 252f, 1f), 10f);
        cameraTransform.DOLookAt(Vector3.zero, 10f, AxisConstraint.None, Vector3.up);
        yield return new WaitForSeconds(10f);
        cameraTransform.DOMove(new Vector3(106f, 12f, 78f), 19f);
        cameraTransform.DOLookAt(lookat1_2_vector, 19f, AxisConstraint.None, Vector3.up);
        yield return new WaitForSeconds(19f);
        lookat1_2 = false;
        StartCoroutine(GridOn());
        cameraTransform.DOLookAt(lookat2_3_vector, 8f, AxisConstraint.None, Vector3.up);
        cameraTransform.DOMove(new Vector3(cubes_position[5460].x, 12f, cubes_position[5460].z), 8f);
        yield return new WaitForSeconds(8f);
        cameraTransform.DOLookAt(cubes_position[5200], 2f, AxisConstraint.None, Vector3.up);
        yield return new WaitForSeconds(2f);
        int counter = 0;
        while (counter < 2700)
        {
            cameraTransform.LookAt(cubes_position[5200 - counter]);
            cameraTransform.DOMove(moveTos[5460 - counter], 0.01f);
            yield return new WaitForSeconds(0.01f);
            counter += 10;
        }
        cameraTransform.DOLookAt(lookat0_1_vector, 3f, AxisConstraint.None, Vector3.up);
        yield return new WaitForSeconds(3f);
        StartCoroutine(GridOff());
        lookat0_1 = true;
        cameraTransform.DOMove(new Vector3(cameraStartPoint.x, cameraStartPoint.y + 300f, cameraStartPoint.z), 6f);
        yield return new WaitForSeconds(6f);
        lookat0_1 = false;
        CameraRepeatMove();
    }
}
