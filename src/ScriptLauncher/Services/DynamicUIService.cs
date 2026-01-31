using ScriptLauncher.Models;
using ScriptLauncher.Services.Interfaces;
using ScriptLauncher.ViewModels;

namespace ScriptLauncher.Services;

/// <summary>
/// Builds dynamic UI from script field definitions.
/// </summary>
public class DynamicUIService : IDynamicUIService
{
    private readonly Dictionary<FieldType, Func<FieldDefinition, FieldViewModel, View>> _fieldFactories;

    public DynamicUIService()
    {
        _fieldFactories = new()
        {
            [FieldType.Text] = CreateTextField,
            [FieldType.Multiline] = CreateMultilineField,
            [FieldType.Number] = CreateNumberField,
            [FieldType.Dropdown] = CreateDropdownField,
            [FieldType.Combobox] = CreateDropdownField, // Same as dropdown for now
            [FieldType.Radio] = CreateRadioField,
            [FieldType.Checkbox] = CreateCheckboxField,
            [FieldType.Toggle] = CreateToggleField,
            [FieldType.Date] = CreateDateField,
            [FieldType.DateTime] = CreateDateTimeField,
            [FieldType.Password] = CreatePasswordField,
            [FieldType.Slider] = CreateSliderField,
            [FieldType.Hidden] = CreateHiddenField,
            [FieldType.File] = CreateTextField, // Simplified for now
            [FieldType.Folder] = CreateTextField, // Simplified for now
            [FieldType.Chips] = CreateTextField, // Simplified - comma separated
            [FieldType.CheckboxGroup] = CreateCheckboxGroupField,
        };
    }

    public View BuildForm(IEnumerable<FieldDefinition> fields, ScriptPopupViewModel viewModel)
    {
        var container = new VerticalStackLayout
        {
            Spacing = 16
        };

        foreach (var field in fields)
        {
            if (field.Type == FieldType.Hidden) continue;

            var fieldVm = viewModel.GetField(field.Id);
            if (fieldVm == null) continue;

            var fieldContainer = CreateFieldContainer(field, fieldVm);
            container.Children.Add(fieldContainer);
        }

        return container;
    }

    public View CreateField(FieldDefinition field, FieldViewModel fieldViewModel)
    {
        if (_fieldFactories.TryGetValue(field.Type, out var factory))
        {
            return factory(field, fieldViewModel);
        }

        // Default to text field
        return CreateTextField(field, fieldViewModel);
    }

    private View CreateFieldContainer(FieldDefinition field, FieldViewModel fieldVm)
    {
        var container = new VerticalStackLayout { Spacing = 4 };

        // Label
        if (!string.IsNullOrEmpty(field.Label))
        {
            var label = new Label
            {
                Text = field.Required ? $"{field.Label} *" : field.Label,
                FontAttributes = FontAttributes.Bold,
                FontSize = 14
            };
            container.Children.Add(label);
        }

        // Control
        var control = CreateField(field, fieldVm);
        container.Children.Add(control);

        // Validation message
        var validationLabel = new Label
        {
            TextColor = Colors.Red,
            FontSize = 12
        };
        validationLabel.SetBinding(Label.TextProperty,
            new Binding(nameof(FieldViewModel.ValidationError), source: fieldVm));
        validationLabel.SetBinding(Label.IsVisibleProperty,
            new Binding(nameof(FieldViewModel.HasError), source: fieldVm));
        container.Children.Add(validationLabel);

        return container;
    }

    private View CreateTextField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var entry = new Entry
        {
            Placeholder = field.Placeholder ?? "",
            MaxLength = field.Validation?.MaxLength ?? int.MaxValue
        };

        entry.SetBinding(Entry.TextProperty,
            new Binding(nameof(FieldViewModel.StringValue), BindingMode.TwoWay, source: fieldVm));

        if (field.Autofocus)
        {
            entry.Focus();
        }

        return entry;
    }

    private View CreateMultilineField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var editor = new Editor
        {
            Placeholder = field.Placeholder ?? "",
            HeightRequest = (field.Rows ?? 4) * 24,
            AutoSize = EditorAutoSizeOption.TextChanges
        };

        editor.SetBinding(Editor.TextProperty,
            new Binding(nameof(FieldViewModel.StringValue), BindingMode.TwoWay, source: fieldVm));

        var frame = new Frame
        {
            Padding = 8,
            BorderColor = Colors.LightGray,
            CornerRadius = 4,
            Content = editor
        };

        return frame;
    }

    private View CreateNumberField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var entry = new Entry
        {
            Placeholder = field.Placeholder ?? "",
            Keyboard = Keyboard.Numeric
        };

        entry.SetBinding(Entry.TextProperty,
            new Binding(nameof(FieldViewModel.StringValue), BindingMode.TwoWay, source: fieldVm));

        return entry;
    }

    private View CreateDropdownField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var picker = new Picker
        {
            Title = field.Placeholder ?? "Select..."
        };

        if (field.Options != null)
        {
            foreach (var option in field.Options)
            {
                picker.Items.Add(option.Label);
            }
        }

        picker.SelectedIndexChanged += (s, e) =>
        {
            if (picker.SelectedIndex >= 0 && field.Options != null)
            {
                fieldVm.Value = field.Options[picker.SelectedIndex].Value;
            }
        };

        // Set initial selection
        if (fieldVm.Value != null && field.Options != null)
        {
            var index = field.Options.FindIndex(o => o.Value == fieldVm.Value.ToString());
            if (index >= 0) picker.SelectedIndex = index;
        }

        return picker;
    }

    private View CreateRadioField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var container = new VerticalStackLayout { Spacing = 8 };
        var groupName = $"radio_{field.Id}";

        if (field.Options == null) return container;

        foreach (var option in field.Options)
        {
            var radio = new RadioButton
            {
                Content = option.Label,
                Value = option.Value,
                GroupName = groupName,
                IsChecked = fieldVm.Value?.ToString() == option.Value
            };

            radio.CheckedChanged += (s, e) =>
            {
                if (e.Value)
                {
                    fieldVm.Value = option.Value;
                }
            };

            container.Children.Add(radio);
        }

        return container;
    }

    private View CreateCheckboxField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var checkbox = new CheckBox();

        checkbox.SetBinding(CheckBox.IsCheckedProperty,
            new Binding(nameof(FieldViewModel.BoolValue), BindingMode.TwoWay, source: fieldVm));

        var layout = new HorizontalStackLayout
        {
            Spacing = 8,
            Children = { checkbox }
        };

        return layout;
    }

    private View CreateToggleField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var toggle = new Switch();

        toggle.SetBinding(Switch.IsToggledProperty,
            new Binding(nameof(FieldViewModel.BoolValue), BindingMode.TwoWay, source: fieldVm));

        return toggle;
    }

    private View CreateDateField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var datePicker = new DatePicker();

        datePicker.DateSelected += (s, e) =>
        {
            fieldVm.Value = e.NewDate;
        };

        if (fieldVm.Value is DateTime dt)
        {
            datePicker.Date = dt;
        }

        return datePicker;
    }

    private View CreateDateTimeField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var container = new HorizontalStackLayout { Spacing = 8 };

        var datePicker = new DatePicker();
        var timePicker = new TimePicker();

        void UpdateValue()
        {
            fieldVm.Value = datePicker.Date.Add(timePicker.Time);
        }

        datePicker.DateSelected += (s, e) => UpdateValue();
        timePicker.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(TimePicker.Time)) UpdateValue();
        };

        container.Children.Add(datePicker);
        container.Children.Add(timePicker);

        return container;
    }

    private View CreatePasswordField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var entry = new Entry
        {
            Placeholder = field.Placeholder ?? "",
            IsPassword = true
        };

        entry.SetBinding(Entry.TextProperty,
            new Binding(nameof(FieldViewModel.StringValue), BindingMode.TwoWay, source: fieldVm));

        return entry;
    }

    private View CreateSliderField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var container = new Grid
        {
            ColumnDefinitions = new ColumnDefinitionCollection
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(GridLength.Auto)
            }
        };

        var slider = new Slider
        {
            Minimum = field.Min ?? 0,
            Maximum = field.Max ?? 100
        };

        var valueLabel = new Label
        {
            WidthRequest = 50,
            HorizontalTextAlignment = TextAlignment.End
        };

        slider.SetBinding(Slider.ValueProperty,
            new Binding(nameof(FieldViewModel.DoubleValue), BindingMode.TwoWay, source: fieldVm));

        valueLabel.SetBinding(Label.TextProperty,
            new Binding(nameof(FieldViewModel.DoubleValue), source: fieldVm,
                stringFormat: "{0:F0}"));

        container.Add(slider, 0, 0);
        container.Add(valueLabel, 1, 0);

        return container;
    }

    private View CreateHiddenField(FieldDefinition field, FieldViewModel fieldVm)
    {
        // Hidden fields don't render anything
        return new ContentView { IsVisible = false };
    }

    private View CreateCheckboxGroupField(FieldDefinition field, FieldViewModel fieldVm)
    {
        var container = new VerticalStackLayout { Spacing = 8 };

        if (field.Options == null) return container;

        var selectedValues = new HashSet<string>();
        if (fieldVm.Value is IEnumerable<string> existing)
        {
            foreach (var v in existing) selectedValues.Add(v);
        }

        foreach (var option in field.Options)
        {
            var checkboxLayout = new HorizontalStackLayout { Spacing = 8 };

            var checkbox = new CheckBox
            {
                IsChecked = selectedValues.Contains(option.Value)
            };

            var label = new Label
            {
                Text = option.Label,
                VerticalOptions = LayoutOptions.Center
            };

            checkbox.CheckedChanged += (s, e) =>
            {
                if (e.Value)
                    selectedValues.Add(option.Value);
                else
                    selectedValues.Remove(option.Value);

                fieldVm.Value = selectedValues.ToList();
            };

            checkboxLayout.Children.Add(checkbox);
            checkboxLayout.Children.Add(label);
            container.Children.Add(checkboxLayout);
        }

        return container;
    }
}
