using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class OpeningManager : UdonSharpBehaviour
{
    [Header("References")]
    [Tooltip("UI演出を行うスクリプト")]
    public IntroSequencer introSequencer;

    [Header("Player Settings")]
    [Tooltip("開始時にプレイヤーを移動させる場所（空のGameObjectなどを指定）")]
    public Transform spawnPoint;
    [Tooltip("演出中に移動を禁止するか")]
    public bool lockMovement = true;

    [Header("Player Defaults (Standard Values)")]
    [Tooltip("通常時の歩行速度 (VRChat標準: 2)")]
    public float defaultWalkSpeed = 2f;
    [Tooltip("通常時の走行速度 (VRChat標準: 4)")]
    public float defaultRunSpeed = 4f;
    [Tooltip("通常時の横歩き速度 (VRChat標準: 2)")]
    public float defaultStrafeSpeed = 2f;
    [Tooltip("通常時のジャンプ力 (VRChat標準: 3)")]
    public float defaultJumpImpulse = 3f;

    void Start()
    {
        // ローカルプレイヤーに対してのみ実行
        VRCPlayerApi player = Networking.LocalPlayer;
        if (player != null)
        {
            // 1. 指定位置へ強制移動
            if (spawnPoint != null)
            {
                player.TeleportTo(spawnPoint.position, spawnPoint.rotation);
            }

            // 2. 移動ロック
            if (lockMovement)
            {
                SetPlayerLocomotion(player, false);
            }
        }

        // 3. UI演出を開始させる
        if (introSequencer != null)
        {
            introSequencer.PlaySequence();
        }
    }

    public void OnIntroFinished()
    {
        // 演出が終わったのでプレイヤーを解放
        VRCPlayerApi player = Networking.LocalPlayer;
        if (player != null && lockMovement)
        {
            SetPlayerLocomotion(player, true);
        }
    }

    private void SetPlayerLocomotion(VRCPlayerApi player, bool isEnabled)
    {
        if (isEnabled)
        {
            // インスペクターで設定した値に戻す
            player.SetWalkSpeed(defaultWalkSpeed);
            player.SetRunSpeed(defaultRunSpeed);
            player.SetStrafeSpeed(defaultStrafeSpeed);
            player.SetJumpImpulse(defaultJumpImpulse);
        }
        else
        {
            // 完全に動けなくする
            player.SetWalkSpeed(0f);
            player.SetRunSpeed(0f);
            player.SetStrafeSpeed(0f);
            player.SetJumpImpulse(0f);
        }
    }
}