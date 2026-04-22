using RestaurantPOS.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace RestaurantPOS.Services
{
    public class ZReportBuilder
    {
        private readonly SettingsService _settingsService;
        private readonly string _currency;

        private const double ReceiptWidth = 260;

        public ZReportBuilder(SettingsService settingsService)
        {
            _settingsService = settingsService;
            _currency = settingsService.Settings.CurrencySymbol;
        }

        public FlowDocument Build(ZReportData data)
        {
            var doc = new FlowDocument
            {
                PageWidth = 280,
                PagePadding = new Thickness(5),
                ColumnGap = 0,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 10
            };

            BuildHeader(doc, data);
            BuildOverall(doc, data);
            BuildByOrderType(doc, data);
            BuildByTill(doc, data);
            BuildByUser(doc, data);
            BuildFooter(doc);

            return doc;
        }

        // ─── Sections ─────────────────────────────────────────

        private void BuildHeader(FlowDocument doc, ZReportData data)
        {
            doc.Blocks.Add(Title(_settingsService.Settings.BusinessName));
            doc.Blocks.Add(Center(_settingsService.Settings.AddressLine1));
            doc.Blocks.Add(Center($"{_settingsService.Settings.City} {_settingsService.Settings.Postcode}"));
            doc.Blocks.Add(Center($"Tel: {_settingsService.Settings.PhoneNo}"));

            doc.Blocks.Add(Spacer(4));
            doc.Blocks.Add(Title("*** Z-REPORT ***"));
            doc.Blocks.Add(Spacer(2));

            doc.Blocks.Add(TwoColumn("From:", data.From.ToString("dd/MM HH:mm")));
            doc.Blocks.Add(TwoColumn("To:", data.To.ToString("dd/MM HH:mm")));
            doc.Blocks.Add(TwoColumn("Printed:", DateTime.Now.ToString("dd/MM HH:mm")));
            doc.Blocks.Add(TwoColumn("User:", data.GeneratedBy));
            doc.Blocks.Add(TwoColumn("Till:", data.TillNo));

            if (!string.IsNullOrWhiteSpace(_settingsService.Settings.VatNo))
                doc.Blocks.Add(TwoColumn("VAT:", _settingsService.Settings.VatNo));

            doc.Blocks.Add(Spacer(2));
            doc.Blocks.Add(DoubleLine());
        }

        private void BuildOverall(FlowDocument doc, ZReportData data)
        {
            var s = data.Overall;

            doc.Blocks.Add(Spacer(2));
            doc.Blocks.Add(SectionHeader("OVERALL"));
            doc.Blocks.Add(Line());

            doc.Blocks.Add(TwoColumn("Orders:", s.OrderCount.ToString()));
            doc.Blocks.Add(TwoColumn("Cancel:", s.CancelCount.ToString()));
            doc.Blocks.Add(TwoColumn("Adults:", s.AdultCovers.ToString()));
            doc.Blocks.Add(TwoColumn("Children:", s.ChildCovers.ToString()));

            doc.Blocks.Add(Spacer(2));
            doc.Blocks.Add(TwoColumn("Cash:", Fmt(s.CashTotal)));
            doc.Blocks.Add(TwoColumn("Card:", Fmt(s.CardTotal)));

            if (s.DepositTotal > 0)
                doc.Blocks.Add(TwoColumn("Deposit:", Fmt(s.DepositTotal)));

            doc.Blocks.Add(Line());
            doc.Blocks.Add(TwoColumnBold("TOTAL:", Fmt(s.GrandTotal)));
            doc.Blocks.Add(TwoColumn("Avg:", Fmt(s.AverageOrder)));

            doc.Blocks.Add(Spacer(2));
            doc.Blocks.Add(DoubleLine());
        }

        private void BuildByOrderType(FlowDocument doc, ZReportData data)
        {
            doc.Blocks.Add(Spacer(2));
            doc.Blocks.Add(SectionHeader("ORDER TYPE"));
            doc.Blocks.Add(Line());

            foreach (var s in data.ByOrderType)
            {
                if (s.OrderCount == 0) continue;

                doc.Blocks.Add(SubHeader(s.Label.ToUpper()));
                BuildSectionRows(doc, s);
                doc.Blocks.Add(Spacer(2));
            }

            doc.Blocks.Add(DoubleLine());
        }

        private void BuildByTill(FlowDocument doc, ZReportData data)
        {
            doc.Blocks.Add(Spacer(2));
            doc.Blocks.Add(SectionHeader("BY TILL"));
            doc.Blocks.Add(Line());

            foreach (var s in data.ByTill)
            {
                doc.Blocks.Add(SubHeader(s.Label.ToUpper()));
                BuildSectionRows(doc, s);
                doc.Blocks.Add(Spacer(2));
            }

            doc.Blocks.Add(DoubleLine());
        }

        private void BuildByUser(FlowDocument doc, ZReportData data)
        {
            doc.Blocks.Add(Spacer(2));
            doc.Blocks.Add(SectionHeader("BY STAFF"));
            doc.Blocks.Add(Line());

            foreach (var s in data.ByUser)
            {
                doc.Blocks.Add(SubHeader(s.Label.ToUpper()));
                BuildSectionRows(doc, s);
                doc.Blocks.Add(Spacer(2));
            }

            doc.Blocks.Add(DoubleLine());
        }

        private void BuildSectionRows(FlowDocument doc, ZReportSection s)
        {
            doc.Blocks.Add(TwoColumn(" Orders:", s.OrderCount.ToString()));
            doc.Blocks.Add(TwoColumn(" Cash:", Fmt(s.CashTotal)));
            doc.Blocks.Add(TwoColumn(" Card:", Fmt(s.CardTotal)));

            if (s.DepositTotal > 0)
                doc.Blocks.Add(TwoColumn(" Deposit:", Fmt(s.DepositTotal)));

            doc.Blocks.Add(TwoColumnBold(" Total:", Fmt(s.GrandTotal)));
        }

        private void BuildFooter(FlowDocument doc)
        {
            doc.Blocks.Add(Spacer(6));
            doc.Blocks.Add(Center("--- END ---"));
            doc.Blocks.Add(Spacer(2));
            doc.Blocks.Add(Center($"Printed: {DateTime.Now:dd/MM HH:mm}"));
            doc.Blocks.Add(Spacer(10));
        }

        // ─── Helpers ─────────────────────────────────────────

        private string Fmt(decimal value) => $"{_currency}{value:N2}";

        private Paragraph Title(string text) =>
            new Paragraph(new Run(text))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 12,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0)
            };

        private Paragraph SectionHeader(string text) =>
            new Paragraph(new Run(text))
            {
                FontSize = 11,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0)
            };

        private Paragraph SubHeader(string text) =>
            new Paragraph(new Run(text))
            {
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = Brushes.DimGray,
                Margin = new Thickness(0, 2, 0, 1)
            };

        private Paragraph Center(string text) =>
            new Paragraph(new Run(text))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0)
            };

        private Paragraph TwoColumn(string left, string right)
        {
            var grid = new Grid { Width = ReceiptWidth };

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftText = new TextBlock { Text = left };
            var rightText = new TextBlock
            {
                Text = right,
                TextAlignment = TextAlignment.Right
            };

            Grid.SetColumn(leftText, 0);
            Grid.SetColumn(rightText, 1);

            grid.Children.Add(leftText);
            grid.Children.Add(rightText);

            return new Paragraph(new InlineUIContainer(grid))
            {
                Margin = new Thickness(0)
            };
        }

        private Paragraph TwoColumnBold(string left, string right)
        {
            var grid = new Grid { Width = ReceiptWidth };

            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var leftText = new TextBlock
            {
                Text = left,
                FontWeight = FontWeights.Bold
            };

            var rightText = new TextBlock
            {
                Text = right,
                TextAlignment = TextAlignment.Right,
                FontWeight = FontWeights.Bold
            };

            Grid.SetColumn(leftText, 0);
            Grid.SetColumn(rightText, 1);

            grid.Children.Add(leftText);
            grid.Children.Add(rightText);

            return new Paragraph(new InlineUIContainer(grid))
            {
                Margin = new Thickness(0)
            };
        }

        private Paragraph Line() =>
            new Paragraph(new Run(new string('-', 32)))
            {
                Margin = new Thickness(0)
            };

        private Paragraph DoubleLine() =>
            new Paragraph(new Run(new string('=', 32)))
            {
                Margin = new Thickness(0)
            };

        private Paragraph Spacer(double height) =>
            new Paragraph(new Run(string.Empty))
            {
                Margin = new Thickness(0, height, 0, 0)
            };
    }
}
