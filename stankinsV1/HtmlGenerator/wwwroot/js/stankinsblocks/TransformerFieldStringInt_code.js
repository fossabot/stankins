
        Blockly.JavaScript['TransformerFieldStringInt'] = function(block) {
  var text_Name = block.getFieldValue('fldName');
  var value_Name = Blockly.JavaScript.valueToCode(block, 'valName', Blockly.JavaScript.ORDER_ATOMIC);
var realValue_Name =value_Name || "'" + text_Name + "'";
  
  
  var text_OldField = block.getFieldValue('fldOldField');
  var value_OldField = Blockly.JavaScript.valueToCode(block, 'valOldField', Blockly.JavaScript.ORDER_ATOMIC);
var realValue_OldField =value_OldField || "'" + text_OldField + "'";
  
  var text_NewField = block.getFieldValue('fldNewField');
  var value_NewField = Blockly.JavaScript.valueToCode(block, 'valNewField', Blockly.JavaScript.ORDER_ATOMIC);
var realValue_NewField =value_NewField || "'" + text_NewField + "'";
  
  var code ='{';
        code+="Name:"+ realValue_Name+",";
        code+="OldField:"+ realValue_OldField+",";
        code+="NewField:"+ realValue_NewField+",";
code+="$type: 'Transformers.TransformerFieldStringInt, Transformers'"; ;
code +='}\n';
  return  [code, Blockly.JavaScript.ORDER_NONE];
};;
    