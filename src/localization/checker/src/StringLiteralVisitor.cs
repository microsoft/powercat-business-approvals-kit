using System.Text.RegularExpressions;
using Microsoft.PowerFx.Syntax;
using Microsoft.PowerCAT.Localization;

namespace Microsoft.PowerCAT.PowerFx;

public class StringLiteralVisitor : TexlVisitor {

    ScanConfig _config;

    public StringLiteralVisitor(ScanConfig config) {
        _config = config == null ? new ScanConfig() : config;
    }

    public bool FoundMatch {get;set;} = false;

    public List<string> Match {get; private set;} = new List<string>();

    public override void Visit(StrLitNode node) {
        if ( String.IsNullOrEmpty(node.Value) ) {
            return;
        }

        if ( int.TryParse(node.Value, out int intValue) ) {
            return;
        }

        if ( !String.IsNullOrEmpty(node.Value) && node.Value.Trim().Length == 0 ) {
            return;
        }

        if (_config.Settings.IgnoreColors && HasCallParent(node, "ColorValue") ) {
            string regex = @"^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$";
            if ( Regex.IsMatch(node.Value, regex) ) {
                return;
            }
        }

        if ( IsStringPartOfIgnoreVariableSetup(node) ) {
            return;
        }


        if ( IsPartOfIgnore(node) ) {
            return;
        }

        if ( IgnoreRecordProperty(node) ) {
            return;
        }
         
        Match.Add(node.Value);
        FoundMatch = true;
    }

    private bool IsStringPartOfIgnoreVariableSetup(StrLitNode node) {
        // Check if can find parent Set Function
        CallNode setNode = FindCallNode(node,"Set");
        if ( setNode == null ) {
            return false;
        }

        // Should only have 2 arguments
        var args = setNode.Args;
        if ( args.Count != 2 ) {
            return false;
        }

        // check if not a name identifier
        var firstArg = args.ChildNodes[0];
        if ( !( firstArg is FirstNameNode ) ) {
            return false;
        }

        // Check if the identifier should be ignored
        var nameNode = firstArg as FirstNameNode;
        return _config.IgnoreVariableSetup.Contains(nameNode.Ident.Name.Value);
    }

    private CallNode FindCallNode(TexlNode node, string name) {
        if ( node is CallNode ) {
           var call = node as CallNode;
           if ( name.Equals(call.Head.Name) ) {
                return call;
           }
        } 
        if ( node == null ) {
            return null;
        }
        return FindCallNode(node.Parent, name);
    }

    private bool IsPartOfIgnore(TexlNode node) {
        if ( node is CallNode ) {
            var call = node as CallNode;
            if ( _config.IgnoreFunctions.Contains(call.Head.Name) ) {
                return true;
            }
        } 
        if ( node == null ) {
            return false;
        }
        return IsPartOfIgnore(node.Parent);
    }

    private bool HasCallParent(TexlNode node, string name) {
        if ( node is CallNode ) {
            var call = node as CallNode;
            if ( name.Equals(call.Head.Name) ) {
                return true;
            }
        } 
        if ( node == null ) {
            return false;
        }
        return HasCallParent(node.Parent, name);
    }

    private bool IgnoreRecordProperty(TexlNode node) {
        if ( node.Parent != null && node.Parent is RecordNode) {
            var record = node.Parent as RecordNode;
            var index = 0;
            var found = false;
            foreach (var child in record.ChildNodes ) {
                if ( child == node ) {
                    found = true;
                    break;
                }
                index++;
            }
            // Check if record property should be ignored
            if ( found && _config.IgnoreProperties.Contains(record.Ids[index].Name) ) {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Visit <see cref="ErrorNode" /> leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void Visit(ErrorNode node) {

    }   

    /// <summary>
    /// Visit <see cref="BlankNode" /> leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void Visit(BlankNode node) {

    }

    /// <summary>
    /// Visit <see cref="BoolLitNode" /> leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void Visit(BoolLitNode node) {

    }

    /// <summary>
    /// Visit <see cref="NumLitNode" /> leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void Visit(NumLitNode node) {

    }

    /// <summary>
    /// Visit <see cref="DecLitNode" /> leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void Visit(DecLitNode node) {

    }

    /// <summary>
    /// Visit <see cref="FirstNameNode" /> leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void Visit(FirstNameNode node) {

    }

    /// <summary>
    /// Visit <see cref="ParentNode" /> leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void Visit(ParentNode node) {

    }

    /// <summary>
    /// Visit <see cref="SelfNode" /> leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void Visit(SelfNode node) {

    }

    /// <summary>
    /// Post-visit <see cref="StrInterpNode" /> non-leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void PostVisit(StrInterpNode node) {

    }

    /// <summary>
    /// Post-visit <see cref="DottedNameNode" /> non-leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void PostVisit(DottedNameNode node) {

    }

    /// <summary>
    /// Post-visit <see cref="UnaryOpNode" /> non-leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void PostVisit(UnaryOpNode node) {

    }

    /// <summary>
    /// Post-visit <see cref="BinaryOpNode" /> non-leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void PostVisit(BinaryOpNode node) {

    }

    /// <summary>
    /// Post-visit <see cref="VariadicOpNode" /> non-leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void PostVisit(VariadicOpNode node) {

    }

    /// <summary>
    /// Post-visit <see cref="CallNode" /> non-leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void PostVisit(CallNode node) {

    }

    /// <summary>
    /// Post-visit <see cref="ListNode" /> non-leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void PostVisit(ListNode node) {

    }

    /// <summary>
    /// Post-visit <see cref="RecordNode" /> non-leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void PostVisit(RecordNode node) {
        
    }

    /// <summary>
    /// Post-visit <see cref="TableNode" /> non-leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void PostVisit(TableNode node) {

    }

    /// <summary>
    /// Post-visit <see cref="AsNode" /> non-leaf node.
    /// </summary>
    /// <param name="node">The visited node.</param>
    public override void PostVisit(AsNode node) {

    }
}