using RestaurantPOS.Domain.Entities;
using System;
using System;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace RestaurantPOS.Services
{
    public class ReceiptBuilder
    {
        private readonly IPricingService _princingService;

        public ReceiptBuilder(IPricingService pricingService)
        {
            _princingService = pricingService;
        }
        public FlowDocument Build(Order order)
        {
            var AdultCoverTotal = _princingService.AdultCoverRate * order.AdultCovers;
            var ChildCoverTotal = _princingService.ChildCoverRate * order.ChildCovers;
            var GrandTotal = AdultCoverTotal + ChildCoverTotal + order.ItemsTotal;

            var CashPaid = order.Payments
                .Where(p => p.Method == PaymentMethod.Cash.ToString())
                .Sum(p => p.Amount);

            var CardPaid = order.Payments
                .Where(p => p.Method == PaymentMethod.Card.ToString())
                .Sum(p => p.Amount);

            var TotalPaid = CashPaid + CardPaid;


            var doc = new FlowDocument
            {
                PageWidth = 300,
                FontFamily = new FontFamily("Consolas"), // Printer-style
                FontSize = 12,
                PagePadding = new Thickness(10)
            };

            AddLogo(doc);

            // ✅ HEADER
            doc.Blocks.Add(Title("NAWAB PALACE"));
            doc.Blocks.Add(Center("136 Rochdale Rd Burry"));
            doc.Blocks.Add(Center("Tel: 01612170541"));

            doc.Blocks.Add(Spacer(6));
            doc.Blocks.Add(Line());

            // ✅ ORDER META
            doc.Blocks.Add(TwoColumnBold($"Table: {order.TableNumber}",
                                         $"Order: #{order.Id}"));

            doc.Blocks.Add(TwoColumn($"Till: 1", $"User: Admin"));

            doc.Blocks.Add(Center($"{DateTime.Now:dd-MM-yyyy hh:mm tt}"));

            doc.Blocks.Add(Line());

            // ✅ COVERS
            doc.Blocks.Add(SectionHeader("COVERS"));

            doc.Blocks.Add(TwoColumn(
                $"Adults: x{order.AdultCovers} {AdultCoverTotal}",
                $"Children: x{order.ChildCovers} {ChildCoverTotal}"
            ));

            doc.Blocks.Add(Line());

            // ✅ ITEMS
            doc.Blocks.Add(SectionHeader("ITEMS"));
            doc.Blocks.Add(Spacer(4));

            doc.Blocks.Add(BuildItemsTable(order));

            doc.Blocks.Add(Line());

            // ✅ TOTALS
            doc.Blocks.Add(TotalRow("TOTAL", GrandTotal, true));
            doc.Blocks.Add(Spacer(4));

            if(CashPaid > 0)
                doc.Blocks.Add(TotalRow("Cash", CashPaid));

            if(CardPaid > 0)
                doc.Blocks.Add(TotalRow("Card", CardPaid));

            doc.Blocks.Add(Line());

            // ✅ FOOTER
            doc.Blocks.Add(Spacer(6));
            doc.Blocks.Add(CenterBold("Thank You!"));
            doc.Blocks.Add(Spacer(4));

            doc.Blocks.Add(Center("Developed By"));
            doc.Blocks.Add(CenterBold("EPS Tech Mirpur"));

            return doc;
        }

        // ✅ LOGO
        private void AddLogo(FlowDocument doc)
        {
            try
            {
                var image = new Image
                {
                    Source = new BitmapImage(
                        new Uri("pack://application:,,,/Assets/logo.png")),
                    Width = 60,
                    Height = 60
                };

                doc.Blocks.Add(new BlockUIContainer(image)
                {
                    TextAlignment = TextAlignment.Center
                });

                doc.Blocks.Add(Spacer(6));
            }
            catch(Exception ex) {
                Debug.Write(ex.Message);
            }
        }

        // ✅ ITEMS TABLE (THERMAL STYLE)
        private System.Windows.Documents.Table BuildItemsTable(Order order)
        {
            var table = new System.Windows.Documents.Table();

            table.Columns.Add(new TableColumn { Width = new GridLength(100) });
            table.Columns.Add(new TableColumn { Width = new GridLength(60) });
            table.Columns.Add(new TableColumn { Width = new GridLength(80) });

            var group = new TableRowGroup();
            table.RowGroups.Add(group);

            foreach (var item in order.Items)
            {
                var row = new TableRow();

                row.Cells.Add(Cell(item.ProductName));
                row.Cells.Add(Cell($"x{item.Quantity}", TextAlignment.Center));
                row.Cells.Add(Cell($"£{item.LineTotal:F2}", TextAlignment.Right));

                group.Rows.Add(row);
            }

            return table;
        }

        // ✅ TOTAL ROW (BOLD OPTION)
        private Paragraph TotalRow(string label, decimal value, bool bold = false)
        {
            var run = new Run($"{label}");
            if (bold) run.FontWeight = FontWeights.Bold;

            var amount = new Run($"£{value:F2}");
            if (bold)
            {
                amount.FontWeight = FontWeights.Bold;
                amount.FontSize = 14; // Emphasize total
            }

            return new Paragraph
            {
                Margin = new Thickness(0),
                Inlines =
            {
                run,
                new Run("     "),
                amount
            }
            };
        }

        // ✅ TYPOGRAPHY HELPERS ⭐⭐⭐⭐⭐

        private Paragraph Title(string text) =>
            new Paragraph(new Run(text))
            {
                TextAlignment = TextAlignment.Center,
                FontSize = 18,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0)
            };

        private Paragraph SectionHeader(string text) =>
            new Paragraph(new Run(text))
            {
                FontWeight = FontWeights.Bold,
                FontSize = 13,
                Margin = new Thickness(0)
            };

        private Paragraph Center(string text) =>
            new Paragraph(new Run(text))
            {
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0)
            };

        private Paragraph CenterBold(string text) =>
            new Paragraph(new Run(text))
            {
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0)
            };

        private Paragraph TwoColumn(string left, string right) =>
            new Paragraph
            {
                Margin = new Thickness(0),
                Inlines =
                {
                new Run(left),
                new Run("     "),
                new Run(right)
                }
            };

        private Paragraph TwoColumnBold(string left, string right) =>
            new Paragraph
            {
                Margin = new Thickness(0),
                Inlines =
                {
                new Run(left) { FontWeight = FontWeights.Bold },
                new Run("     "),
                new Run(right) { FontWeight = FontWeights.Bold }
                }
            };

        private Paragraph Line() =>
            new Paragraph(new Run("-----------------------------------------"))
            {
                Margin = new Thickness(0)
            };

        private Paragraph Spacer(double height) =>
            new Paragraph(new Run(""))
            {
                Margin = new Thickness(0, height, 0, 0)
            };

        private TableCell Cell(string text,
            TextAlignment alignment = TextAlignment.Left)
        {
            return new TableCell(new Paragraph(new Run(text)))
            {
                TextAlignment = alignment,
                Padding = new Thickness(0),
                BorderThickness = new Thickness(0)
            };
        }
    }
}
