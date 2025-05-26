using Godot;

public static class Events
{

    public static void Setup()
    {
        //quando o jogo inicia
        EventHandler.AddEvent("OnStart", false, () =>
        {
            Player.Instance.InteractDialog("1#_pai_para_arana", "FirstQuest");
            Player.Instance.canAttack = false;
            GameManager.Instance.espirito.dialogPath = "side_fantasma";
            GameManager.Instance.espirito.EventAtEnd = "side_espirito_start";
            GameManager.Instance.moacir.Visible = true;
            GameManager.Instance.moacir.dialogPath = "side_1_moacir";
            GameManager.Instance.moacir.EventAtEnd = "side_moacir_start";

        });
        //apos primeiro dialogo
        EventHandler.AddEvent("FirstQuest", false, () =>
        {
            GameManager.Instance.Apoena.dialogPath = "2#_apoena";
            GameManager.Instance.Apoena.EventAtEnd = "Quest1#Ended";
        });
        //falou com apoena
        EventHandler.AddEvent("Quest1#Ended", false, () =>
        {
            Player.Instance.canAttack = true;
            GameManager.Instance.Apoena.dialogPath = "2_1#_apoena";
            GameManager.Instance.Apoena.EventAtEnd = "";
            GameManager.Instance.Apua.dialogPath = "3#_apua";
            GameManager.Instance.Apua.EventAtEnd = "Quest2#Ended";
        });
        //fala com apua
        EventHandler.AddEvent("Quest2#Ended", false, () =>
        {
            GameManager.Instance.Apua.dialogPath = "3_1#_apua";
            GameManager.Instance.Apua.EventAtEnd = "vereficaSementes";
            Player.Instance.AddGold(10);
            GameManager.Instance.Karai_dialog.dialogPath = "4#_karai";
            GameManager.Instance.Karai_dialog.EventAtEnd = "Quest3#Ended";
        });
        //side quest verefica se tem sementes
        EventHandler.AddEvent("vereficaSementes", false, () =>
        {
            //id da semente
            if (Player.Instance.inventory.Contains(1, 2))
            {
                Player.Instance.SetDialog("3_2#_apua", "");
                GameManager.Instance.Apua.dialogPath = "";
                GameManager.Instance.Apua.EventAtEnd = "";
                Player.Instance.AddGold(5);
                CraftingSystem.DiscoverRecipe("pocao_jatoba");
            }
            else
            {
                Player.Instance.SetDialog("3_3#_apua", "");
            }
        });
        //fala com karai e compra sementes
        EventHandler.AddEvent("Quest3#Ended", false, () =>
        {
            GameManager.Instance.Karai_dialog.Visible = false;
            GameManager.Instance.Karai_Merchant.Visible = true;
            Player.Instance.RemoveGold(10);
            Player.Instance.Add(1, 2);

            GameManager.Instance.Luna.dialogPath = "5#_velhaLuna";
            GameManager.Instance.Luna.EventAtEnd = "Quest4#Ended";
        });
        //fala com luna
        EventHandler.AddEvent("Quest4#Ended", false, () =>
        {
            GameManager.Instance.Luna.dialogPath = "5_1#_velhaLuna";
            GameManager.Instance.Luna.EventAtEnd = "";
            GameManager.Instance.caua.dialogPath = "6#_caua";
            GameManager.Instance.caua.EventAtEnd = "Quest5#Ended";
            CraftingSystem.DiscoverRecipe("pocao_uxi");
            CraftingSystem.DiscoverRecipe("pocao_protetora");
        });
        //fala com caua
        EventHandler.AddEvent("Quest5#Ended", false, () =>
        {
            GameManager.Instance.caua.dialogPath = "6_1#_caua";
            GameManager.Instance.caua.EventAtEnd = "";
            GameManager.Instance.thauan.dialogPath = "7#_thauan";
            GameManager.Instance.thauan.EventAtEnd = "Quest6#Ended";
        });
        //fala com thauan
        EventHandler.AddEvent("Quest6#Ended", false, () =>
        {
            GameManager.Instance.thauan.dialogPath = "";
            GameManager.Instance.thauan.EventAtEnd = "";
            CraftingSystem.DiscoverRecipe("armadura_viva");
        });




        //=========side quests=================
        //=========   moacir  =================

        EventHandler.AddEvent("side_moacir_start", false, () =>
        {
            GameManager.Instance.moacir.dialogPath = "side_2_moacir";
            GameManager.Instance.moacir.EventAtEnd = "side_moacir_end";
        });

        EventHandler.AddEvent("side_moacir_end", false, () =>
        {
            if (GameManager.Instance.moacirQuestEnemies.GetChildCount() == 0)
            {
                Player.Instance.SetDialog("side_3_moacir", "");
                GameManager.Instance.moacir.dialogPath = "";
                GameManager.Instance.moacir.EventAtEnd = "";
                GameManager.Instance.moacir.Visible = false;
                Player.Instance.Add(21);
                Player.Instance.AddGold(25);
            }
            else
            {
                Player.Instance.SetDialog("side_4_moacir", "");
            }
        });
        //=========  espiritos  =================
        EventHandler.AddEvent("side_espirito_start", false, () =>
        {
            GameManager.Instance.espirito.dialogPath = "side_3_fantasma";
            GameManager.Instance.espirito.EventAtEnd = "side_espiritos_end";
        });
        EventHandler.AddEvent("side_espiritos_end", false, () =>
        {
            if (Player.Instance.inventory.Contains(3))
            {
                Player.Instance.SetDialog("side_1_fantasma", "");
                GameManager.Instance.espirito.dialogPath = "";
                GameManager.Instance.espirito.EventAtEnd = "";
                CraftingSystem.DiscoverRecipe("pocao_tucum");
                CraftingSystem.DiscoverRecipe("pocao_urucum");
            }
            else
            {
                Player.Instance.SetDialog("side_2_fantasmas", "");
            }
        });

        // ========= Triggers =================
        EventHandler.AddEvent("boss_start", false, () =>
        {
            Camera.Instance.SetToBoss();
            Boss.Instance.Activate();
            PlayerAudioManager.Instance.PlaySong(PlayerAudioManager.SongToPlay.Boss);
            GameManager.Instance.BossEntrance.GetNode<CollisionShape2D>("CollisionShape2D").Position = new(4.0f, 0);
        });
        EventHandler.AddEvent("vila_do_mar_enter", false, () =>
        {
            PlayerAudioManager.Instance.PlaySong(PlayerAudioManager.SongToPlay.VilaDoMar);
        });
        EventHandler.AddEvent("anhau_enter", false, () =>
        {
            PlayerAudioManager.Instance.PlaySong(PlayerAudioManager.SongToPlay.Anhau);
        });
        EventHandler.AddEvent("overworld_enter", false, () =>
        {
            PlayerAudioManager.Instance.PlaySong(PlayerAudioManager.SongToPlay.Overworld);
        });
    }
}