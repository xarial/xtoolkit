---
title: Creating the Flag Enum Bound ComboBox in WPF using xToolkit
caption: Flag Enum ComboBox
description: Multi choice ComboBox control with flag enumeration binding in xToolkit
image: enum-combo-box.png
---
This WPF control allows binding the fields of the [flag](https://docs.microsoft.com/en-us/dotnet/api/system.flagsattribute?view=netcore-3.1) enumeration to the multi-selection combo box.

![Enumeration bound to combo box items](enum-combo-box.png)

Create flag enumeration with fields to be bound to the combo box

{% code-snippet { file-name: ~wpf/FlagEnumComboBoxDocs.*, regions: [enum] } %}

Default value assigned to title is enumeration name. To assign the custom title inherit [DisplayNameAttribute](https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.displaynameattribute?view=netcore-3.1)

{% code-snippet { file-name: ~wpf/FlagEnumComboBoxDocs.*, regions: [enum-display-name-attribute] } %}

Bind the View Model property in the XAML to the *Value* dependency property.

{% code-snippet { file-name: ~wpf/FlagEnumComboBoxDocs.*, regions: [view-model] } %}

{% code-snippet { file-name: ~wpf/FlagEnumComboBoxControl.xaml, lang: xml } %}

Control supports joined values as well as none (0) value. These items will be assigned with blue and gray colors correspondingly.