using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class Tester : MonoBehaviour
{
    #region Inspector Variables

    #endregion

    #region Public Variables

    #endregion

    #region Private Variables

    #endregion

    #region Unity Methods

    private void Start()
    {
        var sb = new StringBuilder();
        var list = new List<string>();
        list.Add("Hola");
        list.Add("Hola");
        list.Add("Hola");
        list.Add("Hola");
        sb.AppendJoin(',', list).AppendLine();
        sb.AppendJoin(',', list).AppendLine();
        Debug.Log(sb.ToString());

        var t = "Hola,,Hello,";
        var l = t.Split(',');
        foreach (var s in l)
        {
            Debug.Log(s);
        }
    }

    #endregion

    #region Public Methods

    #endregion

    #region Private Methods

    #endregion
}