using Sitecore;
using Sitecore.Collections;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Globalization;
using Sitecore.Resources;
using Sitecore.Text.Diff;
using Sitecore.Web.UI.HtmlControls;
using Sitecore.Web.UI.WebControls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SitecoreExtensions.SitecoreItemDiff.Dialogs.ItemDBCompare
{
    class ItemCompareView
    {
        #region DiffView override

        // Sitecore.Text.Diff.View.DiffView
        //Two column view
        public static int CompareTwoColumnView(System.Web.UI.Control parent, Item item1, Item item2, string click)
        {
            int num = 0;
            Item item3 = item1.Versions.GetLatestVersion(Context.Language) ?? item1;
            FieldCollection fields = item1.Fields;
            fields.ReadAll();
            fields.Sort();
            string a = null;
            GridPanel gridPanel = null;
            foreach (Field field in fields)
            {
                if (ShowField(field))
                {
                    Field field2 = item3.Fields[field.Name];
                    if (a != field.Section)
                    {
                        Section section = new Section();
                        section.Class = "scSection";
                        parent.Controls.Add(section);
                        section.Header = field.SectionDisplayName;
                        section.ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("S");
                        gridPanel = new GridPanel();
                        section.Controls.Add(gridPanel);
                        gridPanel.Columns = 2;
                        gridPanel.Width = new Unit(100.0, UnitType.Percentage);
                        gridPanel.Fixed = true;
                        a = field.Section;
                    }
                    string text = GetValue(item1[field.Name]);
                    string text2 = GetValue(item2[field.Name]);
                    string @class = (text == text2) ? "scUnchangedFieldLabel" : "scChangedFieldLabel";
                    //if (field.IsBlobField)
                    //{
                    //    if (text != text2)
                    //    {
                    //        text = "<span style=\"color:blue\">" + Translate.Text("Changed") + "</span>";
                    //        text2 = "<span style=\"color:blue\">" + Translate.Text("Changed") + "</span>";
                    //    }
                    //    else
                    //    {
                    //        text = Translate.Text("Unable to compare binary fields.");
                    //        text2 = Translate.Text("Unable to compare binary fields.");
                    //    }
                    //}
                    //else
                    //{
                        Compare(ref text, ref text2);
                    //}
                    text = ((text.Length == 0) ? "&#160;" : text);
                    text2 = ((text2.Length == 0) ? "&#160;" : text2);
                    Border border = new Border();
                    gridPanel.Controls.Add(border);
                    gridPanel.SetExtensibleProperty(border, "valign", "top");
                    gridPanel.SetExtensibleProperty(border, "width", "50%");
                    border.Class = @class;
                    border.Controls.Add(new LiteralControl(field2.DisplayName + ":"));
                    border = new Border();
                    gridPanel.Controls.Add(border);
                    gridPanel.SetExtensibleProperty(border, "valign", "top");
                    gridPanel.SetExtensibleProperty(border, "width", "50%");
                    border.Class = @class;
                    border.Controls.Add(new LiteralControl(field2.DisplayName + ":"));
                    border = new Border();
                    gridPanel.Controls.Add(border);
                    gridPanel.SetExtensibleProperty(border, "valign", "top");
                    border.Class = "scField";
                    if (field.Type == "checkbox")
                    {
                        text = "<input type=\"checkbox\" disabled" + ((text == "1") ? " checked" : string.Empty) + " />";
                    }
                    border.Controls.Add(new LiteralControl(text));
                    border = new Border();
                    gridPanel.Controls.Add(border);
                    gridPanel.SetExtensibleProperty(border, "valign", "top");
                    border.Class = "scField";
                    if (field.Type == "checkbox")
                    {
                        text2 = "<input type=\"checkbox\" disabled" + ((text2 == "1") ? " checked" : string.Empty) + " />";
                    }
                    border.Controls.Add(new LiteralControl(text2));
                    num++;
                }
            }
            return num;
        }

        //One column view
        public static int CompareOneColumnView(System.Web.UI.Control parent, Item item1, Item item2, string click)
        {
            int num = 0;
            Section section = null;
            Item item3 = item1.Versions.GetLatestVersion(Context.Language) ?? item1;
            FieldCollection fields = item1.Fields;
            fields.ReadAll();
            fields.Sort();
            string a = null;
            foreach (Field field in fields)
            {
                Field field2 = item3.Fields[field.Name];
                if (ShowField(field))
                {
                    if (a != field.Section)
                    {
                        section = new Section
                        {
                            Class = "scSection"
                        };
                        parent.Controls.Add(section);
                        section.Header = field.SectionDisplayName;
                        section.ID = Sitecore.Web.UI.HtmlControls.Control.GetUniqueID("S");
                        a = field.Section;
                    }
                    string value = GetValue(item1[field.Name]);
                    string value2 = GetValue(item2[field.Name]);
                    string @class = (value == value2) ? "scUnchangedFieldLabel" : "scChangedFieldLabel";
                    string text;
                    if (field.IsBlobField)
                    {
                        if (value != value2)
                        {
                            text = "<span style=\"color:blue\">" + Translate.Text("Changed") + "</span>";
                        }
                        else
                        {
                            text = Translate.Text("Unable to compare binary fields.");
                        }
                    }
                    else
                    {
                        text = Compare(value, value2);
                    }
                    text = ((text.Length == 0) ? "&#160;" : text);
                    Border border = new Border();
                    section.Controls.Add(border);
                    border.Class = @class;
                    border.Controls.Add(new LiteralControl(field2.DisplayName + ":"));
                    GridPanel gridPanel = new GridPanel();
                    section.Controls.Add(gridPanel);
                    gridPanel.Columns = 2;
                    gridPanel.Width = new Unit(100.0, UnitType.Percentage);
                    border = new Border();
                    gridPanel.Controls.Add(border);
                    border.Class = "scField";
                    if (field.Type == "checkbox")
                    {
                        text = "<input type=\"checkbox\" disabled" + ((text == "1") ? " checked" : string.Empty) + " />";
                    }
                    border.Controls.Add(new LiteralControl(text));
                    Border border2 = new Border();
                    gridPanel.Controls.Add(border2);
                    if (click.Length > 0)
                    {
                        gridPanel.SetExtensibleProperty(border2, "width", "16px");
                        gridPanel.SetExtensibleProperty(border2, "valign", "top");
                        border2.ToolTip = Translate.Text("Translate");
                        border2.Click = string.Concat(new object[]
                        {
                    click,
                    "(\"",
                    field.ID,
                    "\")"
                        });
                        ImageBuilder imageBuilder = new ImageBuilder
                        {
                            Src = "Applications/16x16/nav_right_green.png",
                            Width = 16,
                            Height = 16,
                            Margin = "4px 0px 0px 0px",
                            Alt = border2.ToolTip
                        };
                        border2.Controls.Add(new LiteralControl(imageBuilder.ToString()));
                    }
                    num++;
                }
            }
            return num;
        }

        protected static string Compare(string value1, string value2)
        {
            DiffEngine diffEngine = new DiffEngine();
            value1 = StringUtil.RemoveTags(value1);
            value2 = StringUtil.RemoveTags(value2);
            DiffListHtml source = new DiffListHtml(value1);
            DiffListHtml destination = new DiffListHtml(value2);
            diffEngine.ProcessDiff(source, destination, DiffEngineLevel.SlowPerfect);
            System.Collections.ArrayList arrayList = diffEngine.DiffReport();
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < arrayList.Count; i++)
            {
                DiffResultSpan diffResultSpan = arrayList[i] as DiffResultSpan;
                if (diffResultSpan != null)
                {
                    switch (diffResultSpan.Status)
                    {
                        case DiffResultSpanStatus.NoChange:
                            Append(stringBuilder, value1, diffResultSpan.SourceIndex, diffResultSpan.Length);
                            break;
                        case DiffResultSpanStatus.Replace:
                            Append(stringBuilder, value1, diffResultSpan.SourceIndex, diffResultSpan.Length, "green");
                            Append(stringBuilder, value2, diffResultSpan.DestIndex, diffResultSpan.Length, "red; text-decoration:line-through;font-weight:bold");
                            break;
                        case DiffResultSpanStatus.DeleteSource:
                            Append(stringBuilder, value1, diffResultSpan.SourceIndex, diffResultSpan.Length, "green;font-weight:bold");
                            break;
                        case DiffResultSpanStatus.AddDestination:
                            Append(stringBuilder, value2, diffResultSpan.DestIndex, diffResultSpan.Length, "red; text-decoration:line-through;font-weight:bold");
                            break;
                    }
                }
            }
            return stringBuilder.ToString();
        }

        protected static bool ShowField(Field field)
        {
            //Todo- Ignore the standard fields based on the settings.
            //return field.ShouldBeTranslated;
            return true;
        }

        // Sitecore.Text.Diff.View.TwoCoumnsDiffView
        protected static void Compare(ref string value1, ref string value2)
        {
            DiffEngine diffEngine = new DiffEngine();
            string value3 = value1;
            string value4 = value2;
            value1 = StringUtil.RemoveTags(value1);
            value2 = StringUtil.RemoveTags(value2);
            DiffListHtml source = new DiffListHtml(value1);
            DiffListHtml destination = new DiffListHtml(value2);
            diffEngine.ProcessDiff(source, destination, DiffEngineLevel.SlowPerfect);
            System.Collections.ArrayList arrayList = diffEngine.DiffReport();
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            System.Text.StringBuilder stringBuilder2 = new System.Text.StringBuilder();
            for (int i = 0; i < arrayList.Count; i++)
            {
                DiffResultSpan diffResultSpan = arrayList[i] as DiffResultSpan;
                if (diffResultSpan != null)
                {
                    switch (diffResultSpan.Status)
                    {
                        case DiffResultSpanStatus.NoChange:
                            Append(stringBuilder, value1, diffResultSpan.SourceIndex, diffResultSpan.Length);
                            Append(stringBuilder2, value2, diffResultSpan.DestIndex, diffResultSpan.Length);
                            break;
                        case DiffResultSpanStatus.Replace:
                            Append(stringBuilder, value1, diffResultSpan.SourceIndex, diffResultSpan.Length, "blue;font-weight:bold");
                            Append(stringBuilder2, value2, diffResultSpan.DestIndex, diffResultSpan.Length, "blue;font-weight:bold");
                            break;
                        case DiffResultSpanStatus.DeleteSource:
                            Append(stringBuilder, value1, diffResultSpan.SourceIndex, diffResultSpan.Length, "blue;font-weight:bold");
                            Append(stringBuilder2, value2, diffResultSpan.DestIndex, diffResultSpan.Length, "blue;font-weight:bold");
                            break;
                        case DiffResultSpanStatus.AddDestination:
                            Append(stringBuilder, value1, diffResultSpan.SourceIndex, diffResultSpan.Length, "blue;font-weight:bold");
                            Append(stringBuilder2, value2, diffResultSpan.DestIndex, diffResultSpan.Length, "blue;font-weight:bold");
                            break;
                    }
                }
            }

            if (arrayList.Count == 0 && (value3.Contains("<image") || value4.Contains("<image") || value3.Contains("<link") || value4.Contains("<link")))
            {
                stringBuilder.Append("<span>" + value3.Replace("<image", "< image").Replace("<link", "< link") + "</span>");
                stringBuilder2.Append("<span>" + value4.Replace("<image", "< image").Replace("<link", "< link") + "</span>");
            }

            value1 = stringBuilder.ToString();
            value2 = stringBuilder2.ToString();
        }

        protected static void Append(System.Text.StringBuilder builder, string value, int index, int length)
        {
            if (length > 0 && index >= 0)
            {
                builder.Append(StringUtil.Mid(value, index, length));
            }
        }

        protected static void Append(System.Text.StringBuilder builder, string value, int index, int length, string color)
        {
            if (length > 0 && index >= 0)
            {
                builder.Append("<span style=\"color:" + color + "\">");
                builder.Append(StringUtil.Mid(value, index, length));
                builder.Append("</span>");
            }
        }

        protected static string GetValue(string value)
        {
            string text = string.Empty;

            if (string.IsNullOrEmpty(value))
                return value;

            //check date value
            if (DateUtil.IsIsoDate(value))
            {
                return DateUtil.FormatLongDateTime(DateUtil.IsoDateToDateTime(value));
            }

            //check boolean value
            if (value == "1")
            { text = "true"; }
            else if (value == "0")
            { text = "false"; }
            else
            {
                text = value;
            }

            //string[] array3 = array;
            //for (int j = 0; j < array3.Length; j++)
            //{
            //    string itemPath = array3[j];
            //    text += ((text.Length > 0) ? ", " : "");
            //    Item item = Context.ContentDatabase.Items[itemPath];
            //    if (item != null)
            //    {
            //        text += item.DisplayName;
            //    }
            //    else
            //    {
            //        text += Translate.Text("[unknown]");
            //    }
            //}
            return text;
        }

        #endregion
    }
}
