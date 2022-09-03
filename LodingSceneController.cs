using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class LodingSceneController : MonoBehaviour
{
    //LodingSceneController �ν��Ͻ� ����
    private static LodingSceneController instance;

    //LodingSceneController �ν��Ͻ� ������ �Լ� (�̱������� �۾��ؾ��ϴ� ������ ��������� �ʵ��� ����)
    public static LodingSceneController Instance
    {
        get
        {
            Debug.Log("LodingSceneController \nInstance �۵���");
            if (instance == null)
            {
                LodingSceneController obj = FindObjectOfType<LodingSceneController>();
                if(obj != null)
                {
                    instance = obj;
                }
                else
                {
                    instance = Create();
                }
            }
            Debug.Log("LodingSceneController \nInstance �۵�����");
            return instance;
        }
    }

    //
    private static LodingSceneController Create()
    {
        Debug.Log("LodingSceneController \nCreate �۵���");
        //Resources.Load�� ������ �ε��ϴ� ��� ���� GameobjectŬ������ �������� �־�� �ʿ� ���� Project�������� �ٷ� ������ �� �ִ�.
        return Instantiate(Resources.Load<LodingSceneController>("LodingUI"));
    }

    private void Awake()
    {
        //�̱��� �ۼ��̱� ������ �������� �ν��Ͻ��� ���� �� �ı��ϰ�, ������Ʈ�� �ϳ��� �����ؾ��ϸ� �� �̵��߿� �ı��Ǹ� �ȵ�
        if (instance != this)
        {
            Debug.Log("LodingSceneController \nUI�ı�");
            if(instance == null)
                Debug.Log("instance : null");
            else
                Debug.Log("instance : " + instance);


            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    //�ε�â���� ����ؾ� �� �������� ��������.
    //SerializeField�� ����Ƽ Inspectorâ���� ������ �����ϵ��� �ϴ� ����̴�. (�����̴� ����ص���)
    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private Image progressBar;

    //�ҷ��� �� �̸��� ����
    private string loadSceneName;

    //�ٸ� �Լ����� Scene�� �ҷ����� ������ ����ϴ� �Լ�
    public void LoadScene(string sceneName)
    {
        Debug.Log("LodingSceneController \nLoadScene �۵���");
        gameObject.SetActive(true);
        //ctrl + . Ű�� �Ա� ���� �� �� �޼��� ���� Ŭ���ϸ� �ڵ����� ����� �ش�. (������� ����!)
        //���� ȣ��Ǹ� ������ �� �޴������� ���̵� �ƿ� �϶�� �ݹ��Լ��� ������
        SceneManager.sceneLoaded += OnScenenLoaded;
        loadSceneName = sceneName;
        StartCoroutine(LoadSceneProcess());
    }

    //���ο� ���� �ҷ������� ��� ���̵� �ƿ��� �����
    private void OnScenenLoaded(Scene arg0, LoadSceneMode arg1)
    {
        //���������� Scene������ �̷�� ���°� Ȯ��
        if (arg0.name == loadSceneName)
        {
            StartCoroutine(Fade(false));

            //�ݹ� ����� �������
            SceneManager.sceneLoaded -= OnScenenLoaded;
        }
    }

    //Scene�� �ҷ����� �ٽ��ڵ�
    private IEnumerator LoadSceneProcess()
    {
        progressBar.fillAmount = 0f;

        //Fade(���̵� ��)�� ������ ��ĥ������ ��ٸ���.
        yield return StartCoroutine(Fade(true));

        //AsyncOperation(���� ����)�� ����Ƽ���� SceneManager�� �񵿱�� Scene�� �ҷ����µ��� �۵��Ǵ� �ڷ�ƾ�̴�. / �̳� ������?
        //LoadScene�� ����ȭ, LoadSceneAsync�� �񵿱�ȭ�� Scene�� �ҷ��´�. / ���� LoadScene�� ��� �� �ʿ� ����.
        AsyncOperation op = SceneManager.LoadSceneAsync(loadSceneName);
        //Scene�ε��� ������ �ٷ� ������� �ʵ��� false�� ����
        op.allowSceneActivation = false;
        
        //�ε��ٰ� �ε巴�� �����̵��� �ϴ� Ÿ�̸�
        float timer = 0.0f;

        //Scene�� �󸶳� �ҷ��Դ��� ������ ���·� ������
        //isdone�� �ε��� �󸶳� ����Ǿ����� �˷��ش�.
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (op.progress < 0.9f)
            {
                //progressBar�� �ε� ���൵�� ǥ���ϰ� �ȴ�.
                progressBar.fillAmount = op.progress;
            }
            //���� ���൵�� 9���� �Ѿ��� ��� progressBar�� �ڵ����� ä������ ����� ������ġ
            else
            {
                timer += Time.unscaledDeltaTime;
                progressBar.fillAmount = Mathf.Lerp(0.9f, 1f, timer);

                //Scene�� �ε��� ������� Sceneȭ���� �����ϱ�
                if(progressBar.fillAmount >= 1f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
        }
    }

 

    private IEnumerator Fade(bool isFadeIn)
    {
        float timer = 0f;
        while(timer <= 1f)
        {
            yield return null;
            //unscaledDeltaTime�� �׳� deltaTime���� �޸� timeScale�� ������ ���� �ʴ´�. (�ٸ����� �������� ���� ���ΰ�)
            timer += Time.unscaledDeltaTime * 3f;
            //isFadeIn���� true�� ȭ���� õõ�� ������ �ƴҰ�� õõ�� �����Ѵ�.
            canvasGroup.alpha = isFadeIn ? Mathf.Lerp(0f, 1f, timer) : Mathf.Lerp(1f, 0f, timer);
        }

        if(!isFadeIn)
        {
            gameObject.SetActive(false);
        }
    }
}
