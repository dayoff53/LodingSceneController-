using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class LodingSceneController : MonoBehaviour
{
    //LodingSceneController 인스턴스 변수
    private static LodingSceneController instance;

    //LodingSceneController 인스턴스 시켜줄 함수 (싱글톤으로 작업해야하는 여러개 만들어지지 않도록 제어)
    public static LodingSceneController Instance
    {
        get
        {
            Debug.Log("LodingSceneController \nInstance 작동됨");
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
            Debug.Log("LodingSceneController \nInstance 작동종료");
            return instance;
        }
    }

    //
    private static LodingSceneController Create()
    {
        Debug.Log("LodingSceneController \nCreate 작동됨");
        //Resources.Load는 에셋을 로드하는 기능 굳이 Gameobject클래스에 프리팹을 넣어둘 필요 없이 Project폴더에서 바로 가져올 수 있다.
        return Instantiate(Resources.Load<LodingSceneController>("LodingUI"));
    }

    private void Awake()
    {
        //싱글톤 작성이기 떄문에 여러개의 인스턴스가 생길 시 파괴하고, 오브젝트는 하나만 존재해야하며 씬 이동중에 파괴되면 안됨
        if (instance != this)
        {
            Debug.Log("LodingSceneController \nUI파괴");
            if(instance == null)
                Debug.Log("instance : null");
            else
                Debug.Log("instance : " + instance);


            Destroy(gameObject);
            return;
        }
        
        DontDestroyOnLoad(gameObject);
    }

    //로딩창에서 사용해야 할 변수들을 가져오자.
    //SerializeField는 유니티 Inspector창에서 관리가 가능하도록 하는 기능이다. (개꿀이니 기억해두자)
    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private Image progressBar;

    //불러올 씬 이름을 저장
    private string loadSceneName;

    //다른 함수에서 Scene을 불러오고 싶을때 사용하는 함수
    public void LoadScene(string sceneName)
    {
        Debug.Log("LodingSceneController \nLoadScene 작동됨");
        gameObject.SetActive(true);
        //ctrl + . 키를 함깨 누를 른 후 메서드 생성 클릭하면 자동으로 만들어 준다. (기억하자 제발!)
        //씬이 호출되면 생성함 씬 메니저에게 페이드 아웃 하라는 콜백함수를 보내줌
        SceneManager.sceneLoaded += OnScenenLoaded;
        loadSceneName = sceneName;
        StartCoroutine(LoadSceneProcess());
    }

    //세로운 씬이 불러와졌을 경울 페이드 아웃을 명령함
    private void OnScenenLoaded(Scene arg0, LoadSceneMode arg1)
    {
        //성공적으로 Scene변경이 이루어 졌는가 확인
        if (arg0.name == loadSceneName)
        {
            StartCoroutine(Fade(false));

            //콜백 지우기 기억하자
            SceneManager.sceneLoaded -= OnScenenLoaded;
        }
    }

    //Scene을 불러오는 핵심코드
    private IEnumerator LoadSceneProcess()
    {
        progressBar.fillAmount = 0f;

        //Fade(페이드 인)이 실행을 마칠때까지 기다린다.
        yield return StartCoroutine(Fade(true));

        //AsyncOperation(동기 조작)은 유니티에서 SceneManager가 비동기로 Scene을 불러오는동안 작동되는 코루틴이다. / 겁나 좋은데?
        //LoadScene는 동기화, LoadSceneAsync는 비동기화로 Scene을 불러온다. / 굳이 LoadScene를 사용 할 필요 없음.
        AsyncOperation op = SceneManager.LoadSceneAsync(loadSceneName);
        //Scene로딩이 끝나도 바로 변경되지 않도록 false로 설정
        op.allowSceneActivation = false;
        
        //로딩바가 부드럽게 움직이도록 하는 타이머
        float timer = 0.0f;

        //Scene을 얼마나 불러왔는지 게이지 형태로 보여줌
        //isdone은 로딩이 얼마나 진행되었는지 알려준다.
        while (!op.isDone)
        {
            yield return null;
            timer += Time.deltaTime;
            if (op.progress < 0.9f)
            {
                //progressBar가 로딩 진행도를 표기하게 된다.
                progressBar.fillAmount = op.progress;
            }
            //만약 진행도가 9할을 넘었을 경우 progressBar가 자동으로 채워지게 만드는 안전장치
            else
            {
                timer += Time.unscaledDeltaTime;
                progressBar.fillAmount = Mathf.Lerp(0.9f, 1f, timer);

                //Scene의 로딩이 끝날경우 Scene화면을 변경하기
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
            //unscaledDeltaTime는 그냥 deltaTime과는 달리 timeScale의 영향을 받지 않는다. (다른에들 버벅여도 지는 따로감)
            timer += Time.unscaledDeltaTime * 3f;
            //isFadeIn값이 true면 화면을 천천히 밝히고 아닐경우 천천히 암전한다.
            canvasGroup.alpha = isFadeIn ? Mathf.Lerp(0f, 1f, timer) : Mathf.Lerp(1f, 0f, timer);
        }

        if(!isFadeIn)
        {
            gameObject.SetActive(false);
        }
    }
}
