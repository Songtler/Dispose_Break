﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using com.TeamPlug.Input;

public class NoGuideChallenge : GameState
{
    private const string CHALLENGE_NAME = "NoGuideChallenge";

    public Transform arrow;
    public Animator tutorial;

    private BallSkin unlockSkin;
    private int level;

    public override IEnumerator Initialize(params object[] _data)
    {
        yield return base.Initialize(_data);

        GetLevelData();
    }

    public override void Begin()
    {
        base.Begin();

        // 방향화살표 설정
        float rad = Mathf.Deg2Rad * ball.degree;
        Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
        float angle = Vector3.Angle(Vector3.right, dir);

        arrow.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    public override void TouchBegan(Vector3 touchPosition, int touchIndex)
    {
        if (tutorial.gameObject.activeSelf)
        {
            tutorial.Play("Fadeout");
        }

        base.TouchBegan(touchPosition, touchIndex);
    }

    public override void Successe()
    {
        SaveData.noGuideClear += 1;

        if (SaveData.noGuideClear == GameManager.Instance.NoGuideCount)
        {
            PopupContoller.Instance.Show("ChallengeCompletePopup", CHALLENGE_NAME);
        }
        else
        {
            PopupContoller.Instance.Show("ChallengeClearPopup", CHALLENGE_NAME, unlockSkin);
        }
    }

    public override void Failed()
    {
        PopupContoller.Instance.Show("ChallengeFailedPopup", CHALLENGE_NAME, RewardCallback);
    }

    public override void Continue()
    {
        blockGroup = new BlockGroup()
        {
            index = 0,
            unlock = 0
        };

        blockGroup.blockIndex = new List<int>();
        blockGroup.blockHp = new List<int>();

        for (int i = 0; i < disposedBlocks.Count; i++)
        {
            if (disposedBlocks[i].isBreaked == false)
            {
                blockGroup.blockIndex.Add(disposedBlocks[i].index);
                blockGroup.blockHp.Add(disposedBlocks[i].hp);
            }
        }

        StartCoroutine(ResetShot());
    }

    public void GetLevelData()
    {
        string where = string.Format("Stage = {0};", SaveData.noGuideClear+1);
        string query = string.Format(Database.SELECT_TABLE_ALL_WHERE, CHALLENGE_NAME, where);

        Database.Query(query, (reader) =>
        {
            level = int.Parse(reader.GetString(0));

            ball.Initialize(float.Parse(reader.GetString(1)));

            blockGroup = new BlockGroup
            {
                index = 0,
                unlock = 0
            };

            string[] index = reader.GetString(2).Split(',');
            string[] hp = reader.GetString(3).Split(',');

            for (int i = 0; i < index.Length; i++)
            {
                blockGroup.blockIndex.Add(int.Parse(index[i]));
                blockGroup.blockHp.Add(int.Parse(hp[i]));
            }
        });

        // 이번스테이지에서 획득할 수 있는 공 스킨
        unlockSkin = GameManager.Instance.ballSkins.Find(x => x.unlockType == 2 && x.unlockLevel == level);
    }
}
