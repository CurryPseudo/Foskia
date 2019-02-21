using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PlayerPropertyCrossScene {
    public float health;
    public SwapCharges swapCharges;
    public TransforPointIndex born;
    public bool faceRight;
    public static PlayerPropertyCrossScene Main {
        get {
            var player = PlayerController.Main;
            if(player == null) return null;
            var pp = PlayerProperty.Main;
            if(pp == null) return null;
            var r = new PlayerPropertyCrossScene();
            r.swapCharges = player.swapCharges;
            r.health = pp.PlayerHealth;
            r.born = pp.born;
            r.faceRight = player.faceRight;
            return r;
        }
        set {
            var player = PlayerController.Main;
            var pp = PlayerProperty.Main;
            var r = value;
            player.swapCharges.Replace(r.swapCharges);
            RedLightPlayerController[] rls = new RedLightPlayerController[player.swapCharges.ChargesCount];
            for(int i = 0; i < rls.Length; i++) {
                rls[i] = RedLightPlayerController.Create(player, player.Position);
            }
            player.swapCharges.ReBind(rls);
            pp.PlayerHealth = r.health;
            pp.born = r.born;
            player.faceRight = r.faceRight;
        }
    }
}