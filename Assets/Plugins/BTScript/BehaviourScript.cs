using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using BTScript.Internal;
using System.Linq;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BTScript {

[CreateAssetMenu]
public class BehaviourScript : ScriptableObject {

    [InfoBox("The script is not yet compiled.", InfoMessageType.Error, "notCompiled")]
    [InfoBox("The compiled script is not up to date.", InfoMessageType.Warning, "notUpToDate")]
    [InfoBox("The script is compiled and up to date!", InfoMessageType.Info, "compileOkay")]
    public TextAsset asset;

    [HideInInspector]
    public ASTNode root;

    [SerializeField, HideInInspector]
    int compiledHash;

    public bool notCompiled {
        get {
            return root == null;
        }
    }

    public bool notUpToDate {
        get {
            return !notCompiled && (!asset || compiledHash != asset.text.GetHashCode());
        }
    }

    public bool compileOkay {
        get {
            return !notCompiled && !notUpToDate;
        }
    }


#if UNITY_EDITOR
    [Button]
    public void Compile() {
        root = Compiler.Compile(asset.text);

        // Remove previous nodes
        var path = AssetDatabase.GetAssetPath(this);
        AssetDatabase.LoadAllAssetsAtPath(path)
            .Where(it => it is ASTNode)
            .ToList()
            .ForEach(it => DestroyImmediate(it, true));
        
        // Save new nodes
        var nodes = new List<ASTNode>();
        PopulateNodes(nodes, root);
        foreach (var node in nodes) {
            node.hideFlags = HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(node, path);
        }

        compiledHash = asset.text.GetHashCode();

        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
    }

    [Button]
    void PrintAST() {
        if (root == null) {
            Debug.Log("Node not compiled!");
        } else {
            var sb = new StringBuilder();
            sb.AppendLine("AST Tree: ");
            root.Print(0, sb);
            Debug.Log(sb.ToString());
        }
    }

    void PopulateNodes(List<ASTNode> nodes, ASTNode cur) {
        nodes.Add(cur);
        if (cur is ASTControlNode) {
            foreach (var child in ((ASTControlNode) cur).children) {
                PopulateNodes(nodes, child);
            }
        } else if (cur is ASTDecoratorNode) {
            PopulateNodes(nodes, ((ASTDecoratorNode) cur).target);
        }
    }
#endif

}

}