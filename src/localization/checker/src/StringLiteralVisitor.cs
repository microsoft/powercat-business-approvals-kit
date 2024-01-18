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

        if ( IsPartOfIgnore(node) ) {
            return;
        }

        if ( IgnoreRecordProperty(node) ) {
            return;
        }

        
        
        Match.Add(node.Value);
        FoundMatch = true;
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