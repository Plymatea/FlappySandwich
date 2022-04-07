using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using CodeMonkey;
using CodeMonkey.Utils;

public class Level : MonoBehaviour
{
    private const float CAMERA_ORTHO_SIZE = 50f;
    private const float PIPE_WIDTH = 7.8f;
    private const float PIPE_HEAD_HEIGHT = 3.75f;
    private const float PIPE_MOVE_SPEED = 30f;
    private const float PIPE_X_DESTROY_POSTION = -100f;
    private const float PIPE_SPAWN_X_POSTION = +100f;
    private const float GROUND_X_DESTROY_POSTION = -200f;
    private const float BIRD_X_POSITION = -20f;
    
    private static Level instance;

    public static Level GetInstance() {
        return instance;
    }

    private List<Transform> groundList;
    private List<Transform> ceilingList;
    private List<Pipe> pipeList;
    private int pipesPassedCount;
    private int pipesSpawned;
    private float pipeSpawnTimer;
    private float pipeSpawnTimerMax;
    private float gapSize;
    private State state;

    public enum Difficulty {
        Easy,
        Medium,
        Hard,
        Impossible,
    }

    private enum State {
        WaitingToStart,
        Playing,
        Dead,
    }

    private void Awake() {
        instance = this;
        pipeList = new List<Pipe>();
        SpawnIntialGround();
        pipeSpawnTimerMax = 1f;
        SetDifficulty(Difficulty.Easy);
        state = State.WaitingToStart;
    }

    private void Start() {
        Bird.GetInstance().OnDied += Bird_OnDied; 
        Bird.GetInstance().OnStartedPlaying += Bird_OnStartedPlaying;
    }

    private void Bird_OnStartedPlaying(object sender, System.EventArgs e) {
        state = State.Playing; 
    }

    private void Bird_OnDied(object sender, System.EventArgs e) {
        // CMDebug.TextPopupMouse("Dead!");
        state = State.Dead;
    }

    private void Update() {
        if (state == State.Playing) {
        HandlePipeMovement();
        HandlePipeSpawning();
        HandleGround();
        };
    }

private void SpawnIntialGround() {
    groundList = new List<Transform>();
    ceilingList = new List<Transform>();
    Transform groundTransform; 
    float groundY = 47.2f;
    float groundWidth = 184f;
    groundTransform = Instantiate(GameAssets.GetInstance().pfGround, new Vector3(0 , -groundY, 0 ), Quaternion.identity);
    groundList.Add(groundTransform);
    groundTransform = Instantiate(GameAssets.GetInstance().pfGround, new Vector3(groundWidth , -groundY, 0 ), Quaternion.identity);
    groundList.Add(groundTransform);
    groundTransform = Instantiate(GameAssets.GetInstance().pfGround, new Vector3(groundWidth * 2f , -groundY, 0 ), Quaternion.identity);
    groundList.Add(groundTransform);
    Transform ceilingTransform;
    ceilingTransform = Instantiate(GameAssets.GetInstance().pfCeiling, new Vector3(0 , groundY, 0 ), Quaternion.identity);
    ceilingList.Add(ceilingTransform);
    ceilingTransform = Instantiate(GameAssets.GetInstance().pfCeiling, new Vector3(groundWidth , groundY, 0 ), Quaternion.identity);
    ceilingList.Add(ceilingTransform);
    ceilingTransform = Instantiate(GameAssets.GetInstance().pfCeiling, new Vector3(groundWidth * 2f, groundY, 0 ), Quaternion.identity);
    ceilingList.Add(ceilingTransform);
}

    private void HandleGround() {
        foreach (Transform groundTransform in groundList) {
            groundTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime;

            if (groundTransform.position.x < GROUND_X_DESTROY_POSTION) {
                float rightMostxPosition = -100f;
                for (int i=0; i < groundList.Count; i++) {
                    if (groundList[i].position.x > rightMostxPosition) {
                        rightMostxPosition = groundList[i].position.x;
                    }
                }
                float groundWidth = 184f;
                groundTransform.position = new Vector3(rightMostxPosition + groundWidth, groundTransform.position.y, groundTransform.position.z);
            }
        }
        foreach (Transform ceilingTransform in ceilingList) {
            ceilingTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime;

            if (ceilingTransform.position.x < GROUND_X_DESTROY_POSTION) {
                float rightMostxPosition = -100f;
                for (int i=0; i < ceilingList.Count; i++) {
                    if (ceilingList[i].position.x > rightMostxPosition) {
                        rightMostxPosition = ceilingList[i].position.x;
                    }
                }
                float groundWidth = 184f;
                ceilingTransform.position = new Vector3(rightMostxPosition + groundWidth, ceilingTransform.position.y, ceilingTransform.position.z);
            }
        }
    }

    private void HandlePipeSpawning() {
        pipeSpawnTimer -= Time.deltaTime;
        if (pipeSpawnTimer < 0) {
            //Time to spawn another pipe
            pipeSpawnTimer += pipeSpawnTimerMax;

            float heightEdgeLimit = 10f;
            float minHeight = gapSize * .5f + heightEdgeLimit;
            float totalHeight = CAMERA_ORTHO_SIZE * 2f;
            float maxHeight = totalHeight - gapSize * .5f - heightEdgeLimit;

            float height = UnityEngine.Random.Range(minHeight, maxHeight);
            CreateGapPipes(height, gapSize, PIPE_SPAWN_X_POSTION);
        }
    }

    private void HandlePipeMovement() {
        for (int i=0; i<pipeList.Count; i++) {
            Pipe pipe = pipeList[i];
            bool isToTheRightOfBird = pipe.GetXPosition() > BIRD_X_POSITION; 
            pipe.Move();
            //Checks the pipe has passed the bird
            if (isToTheRightOfBird && pipe.GetXPosition() <= BIRD_X_POSITION) {
                pipesPassedCount++;
            }
            //Destroy Pipe 
            if (pipe.GetXPosition() < PIPE_X_DESTROY_POSTION) {
                pipe.DestroySelf();
                pipeList.Remove(pipe);
                i--;
            }
        }
    }

    private void SetDifficulty(Difficulty difficulty) {
        switch (difficulty) {
            case Difficulty.Easy:
                gapSize = 50f;
                pipeSpawnTimerMax = 1.2f;
                break;
            case Difficulty.Medium:
                gapSize = 40f;
                pipeSpawnTimerMax = 1.1f;
                break;
            case Difficulty.Hard:
                gapSize = 33f;
                pipeSpawnTimerMax = 1f;
                break;
            case Difficulty.Impossible:
                pipeSpawnTimerMax = .9f;
                gapSize = 25f;
                break;
            
        }
    }
    private Difficulty GetDifficulty () {
        if (pipesSpawned >= 30) return Difficulty.Impossible;
        if (pipesSpawned >= 20) return Difficulty.Hard;
        if (pipesSpawned >= 10) return Difficulty.Medium;
        return Difficulty.Easy;
    }

    private void CreateGapPipes(float gapY, float gapSize, float xPosition) {
        int spriteBody = UnityEngine.Random.Range(0, 2);
        CreatePipe(gapY- gapSize * .5f, xPosition, true, spriteBody);
        CreatePipe(CAMERA_ORTHO_SIZE * 2f - gapY - gapSize * .5f, xPosition, false, spriteBody);
        pipesSpawned++;
        SetDifficulty(GetDifficulty());
    }

    private void CreatePipe(float height, float xPosition, bool createBottom, int spriteBody) {
        //Set Up Head
        Transform pipeHead = Instantiate(GameAssets.GetInstance().pfPipeHead);

        float pipeHeadYPosition;
        if (createBottom) {
            pipeHeadYPosition =  -CAMERA_ORTHO_SIZE + height - PIPE_HEAD_HEIGHT * .5f;
        } else {
            pipeHeadYPosition =  +CAMERA_ORTHO_SIZE - height + PIPE_HEAD_HEIGHT * .5f;            
        }
        pipeHead.position = new Vector3(xPosition, pipeHeadYPosition);

        // Set Up Body
        
        
        Transform pipeBody = Instantiate(GameAssets.GetInstance().spriteArray[spriteBody]);
        float pipeBodyYPosition;
        if (createBottom) {
            pipeBodyYPosition = -CAMERA_ORTHO_SIZE + height * .5f;
        } else {
            pipeBodyYPosition = +CAMERA_ORTHO_SIZE - height * .5f;
            pipeBody.localScale = new Vector3(1, -1, 1);
        }
        pipeBody.position = new Vector3(xPosition, pipeBodyYPosition);


        SpriteRenderer pipeBodySpriteRenderer = pipeBody.GetComponent<SpriteRenderer>();
        pipeBodySpriteRenderer.size = new Vector2(PIPE_WIDTH, height);

        BoxCollider2D pipeBodyCollider = pipeBody.GetComponent<BoxCollider2D>();
        pipeBodyCollider.size = new Vector2(PIPE_WIDTH, height);
        // pipeBodyCollider.offset = new Vector2(0f, -height * .5f);

        Pipe pipe = new Pipe(pipeHead, pipeBody);
        pipeList.Add(pipe);
    }

    private class Pipe {
        private Transform pipeHeadTransform;
        private Transform pipeBodyTransform;

        public Pipe(Transform pipeHeadTransform, Transform pipeBodyTransform) {
            this.pipeBodyTransform = pipeBodyTransform;
            this.pipeHeadTransform = pipeHeadTransform;
        }

    
        public void Move() {
            pipeHeadTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime;
            pipeBodyTransform.position += new Vector3(-1, 0, 0) * PIPE_MOVE_SPEED * Time.deltaTime;
        }

        public float GetXPosition() {
            return pipeHeadTransform.position.x;
        }

        public void DestroySelf() {
            Destroy(pipeHeadTransform.gameObject);
            Destroy(pipeBodyTransform.gameObject);
        }

    }
    public int GetPipesSpawned() {
            return pipesSpawned;
    }

    public int GetPipesPassedCount() {
        return pipesPassedCount;
    }
}
