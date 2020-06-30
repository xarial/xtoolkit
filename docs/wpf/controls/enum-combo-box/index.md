---
title: Creating the Enum Bound ComboBox in WPF using xToolkit
caption: Enum ComboBox
description: Multi choice ComboBox control with flag enumeration binding in xToolkit
image: enum-combo-box.png
---
This WPF control allows binding the fields of the [flag](https://docs.microsoft.com/en-us/dotnet/api/system.flagsattribute?view=netcore-3.1) enumeration to the multi-selection combo box.

![Enumeration bound to combo box items](enum-combo-box.png)

Create flag enumeration with fields to be bound to the combo box

{% code-snippet { file-name: ~wpf/EnumComboBoxDocs.*, regions: [enum] } %}

Bind the View Model property in the XAML to the *Value* dependency property.

{% code-snippet { file-name: ~wpf/EnumComboBoxDocs.*, regions: [view-model] } %}

{% code-snippet { file-name: ~wpf/EnumComboBoxControl.xaml, lang: xml } %}

Control supports joined values as well as none (0) value. These items will be assigned with blue and gray colors correspondingly.