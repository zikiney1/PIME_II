using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public static class DialogManager
{
    const string DIALOG_PATH = "res://Dialogs/";
    static Dictionary<string, string[]> dialogDic = new();

    public static void Setup()
    {
        ReadDialogFile("res://Dialogo/Pai de arana/1#_pai_para_arana.txt");

        ReadDialogFile("res://Dialogo/aponema/2#_apoena.txt");
        ReadDialogFile("res://Dialogo/aponema/2_1#_apoena.txt");

        ReadDialogFile("res://Dialogo/apua/3#_apua.txt");
        ReadDialogFile("res://Dialogo/apua/3_1#_apua.txt");
        ReadDialogFile("res://Dialogo/apua/3_2#_apua.txt");
        ReadDialogFile("res://Dialogo/apua/3_3#_apua.txt");

        ReadDialogFile("res://Dialogo/karai/4#_karai.txt");

        ReadDialogFile("res://Dialogo/luna/5#_velhaLuna.txt");
        ReadDialogFile("res://Dialogo/luna/5_1#_velhaLuna.txt");

        ReadDialogFile("res://Dialogo/caua/6#_caua.txt");

        ReadDialogFile("res://Dialogo/Thauan/7#_thauan.txt");

        ReadDialogFile("res://Dialogo/moacir/side_1_moacir.txt");
        ReadDialogFile("res://Dialogo/moacir/side_2_moacir.txt");
        ReadDialogFile("res://Dialogo/moacir/side_3_moacir.txt");
        ReadDialogFile("res://Dialogo/moacir/side_4_moacir.txt");

        ReadDialogFile("res://Dialogo/fantasma/side_1_fantasma.txt");
        ReadDialogFile("res://Dialogo/fantasma/side_2_fantasmas.txt");
        ReadDialogFile("res://Dialogo/fantasma/side_3_fantasma.txt");
        ReadDialogFile("res://Dialogo/fantasma/side_fantasma.txt");
        
        ReadDialogFile("res://Dialogo/tutorial/tuto_save_stations.txt");
    }
    public static string[] GetDialog(string name) => dialogDic[name];

    static void ReadDialogFile(string path)
    {
        FileAccess file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        string[] DialogSequence = file.GetAsText()
                                .Replace("\r", "")
                                .Split('\n')
                                .Where(x => x != "")
                                .ToArray();
        file.Close();

        string name = path.Split('/').Last().Replace(".txt", "");
        dialogDic.Add(name, DialogSequence);
    }
}
