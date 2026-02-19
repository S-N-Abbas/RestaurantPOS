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
        private readonly UserSessionService _userSessionService;

        public ReceiptBuilder(IPricingService pricingService, UserSessionService userSessionService)
        {
            _princingService = pricingService;
            _userSessionService = userSessionService;
        }
        public FlowDocument Build(Order order)
        {
            // Calculation Logic
            var adultCoverTotal = _princingService.AdultCoverRate * order.AdultCovers;
            var childCoverTotal = _princingService.ChildCoverRate * order.ChildCovers;
            var grandTotal = adultCoverTotal + childCoverTotal + order.ItemsTotal;

            var cashPaid = order.Payments.Where(p => p.Method == "Cash").Sum(p => p.Amount);
            var cardPaid = order.Payments.Where(p => p.Method == "Card").Sum(p => p.Amount);
            var totalPaid = cashPaid + cardPaid;
            var balanceDue = grandTotal - totalPaid;

            var doc = new FlowDocument
            {
                PageWidth = 280, // Standard 80mm thermal width
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                PagePadding = new Thickness(5),
                ColumnGap = 0
            };

            // 1. BRANDING & HEADER
            AddLogo(doc);
            doc.Blocks.Add(Title("NAWAB PALACE"));
            doc.Blocks.Add(Center("136 Rochdale Rd, Bury"));
            doc.Blocks.Add(Center("Tel: 0161 217 0541"));
            doc.Blocks.Add(Spacer(8));
            doc.Blocks.Add(Line());

            // 2. ORDER INFO
            // Using Table for Order Meta to ensure perfect alignment
            var metaTable = new System.Windows.Documents.Table { CellSpacing = 0 };
            metaTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            metaTable.Columns.Add(new TableColumn { Width = new GridLength(1, GridUnitType.Star) });
            var metaRow = new TableRow();
            metaRow.Cells.Add(Cell($"Table: {order.TableNumber}", fontWeight: FontWeights.Bold));
            metaRow.Cells.Add(Cell($"ID: #{order.Id}", TextAlignment.Right));
            metaTable.RowGroups.Add(new TableRowGroup());
            metaTable.RowGroups[0].Rows.Add(metaRow);
            doc.Blocks.Add(metaTable);

            doc.Blocks.Add(TwoColumn($"Server: {_userSessionService?.CurrentUser?.Username}", $"Till: 1"));
            doc.Blocks.Add(Center($"{DateTime.Now:dd/MM/yyyy HH:mm}"));
            doc.Blocks.Add(Line());

            // 3. COVERS
            if (order.AdultCovers > 0 || order.ChildCovers > 0)
            {
                doc.Blocks.Add(SectionHeader("COVERS"));
                if (order.AdultCovers > 0)
                    doc.Blocks.Add(TwoColumn($"Adults (x{order.AdultCovers})", $"£{adultCoverTotal:F2}"));
                if (order.ChildCovers > 0)
                    doc.Blocks.Add(TwoColumn($"Children (x{order.ChildCovers})", $"£{childCoverTotal:F2}"));
                doc.Blocks.Add(Spacer(4));
            }

            // 4. ITEMS
            doc.Blocks.Add(SectionHeader("ITEMS"));
            doc.Blocks.Add(BuildItemsTable(order));
            doc.Blocks.Add(Line());

            // 5. TOTALS (Aligned to Right)
            doc.Blocks.Add(TotalRow("SUBTOTAL", grandTotal));

            if (cashPaid > 0) doc.Blocks.Add(TotalRow("CASH PAID", cashPaid));
            if (cardPaid > 0) doc.Blocks.Add(TotalRow("CARD PAID", cardPaid));

            if (balanceDue > 0)
            {
                doc.Blocks.Add(Spacer(2));
                doc.Blocks.Add(TotalRow("BALANCE DUE", balanceDue, isHighlight: true));
            }
            else if (totalPaid > grandTotal)
            {
                doc.Blocks.Add(TotalRow("CHANGE", totalPaid - grandTotal));
            }

            doc.Blocks.Add(Line());

            // 6. FOOTER
            doc.Blocks.Add(Spacer(10));
            doc.Blocks.Add(CenterBold("THANK YOU FOR YOUR VISIT!"));
            doc.Blocks.Add(Center("VAT No: 123 4567 89")); // Common in UK
            doc.Blocks.Add(Spacer(6));
            doc.Blocks.Add(Center("Developed By"));
            doc.Blocks.Add(CenterBold("EPS Tech Mirpur"));
            doc.Blocks.Add(Spacer(20)); // Extra space for the tear-off

            return doc;
        }

        private Paragraph TotalRow(string label, decimal value, bool isHighlight = false)
        {
            var p = new Paragraph { Margin = new Thickness(0, 2, 0, 2) };
            p.Inlines.Add(new Run(label) { FontWeight = isHighlight ? FontWeights.Black : FontWeights.Normal });

            // This creates a "Right-Aligned" effect for the price
            var priceRun = new Run($"£{value:F2}")
            {
                FontWeight = FontWeights.Bold,
                FontSize = isHighlight ? 16 : 13
            };

            var container = new Figure(new BlockUIContainer(new TextBlock
            {
                Text = $"£{value:F2}",
                FontWeight = priceRun.FontWeight,
                FontSize = priceRun.FontSize,
                TextAlignment = TextAlignment.Right,
                Width = 120 // Fixed width to force right alignment
            }));
            container.HorizontalAnchor = FigureHorizontalAnchor.ColumnRight;

            p.Inlines.Add(new InlineUIContainer(new TextBlock
            {
                Text = $"£{value:F2}",
                Width = 270,
                TextAlignment = TextAlignment.Right,
                FontWeight = priceRun.FontWeight,
                FontSize = priceRun.FontSize
            }));

            return p;
        }

        private Paragraph TwoColumn(string left, string right)
        {
            // Cleanest way to do two columns in FlowDocument without Tables
            return new Paragraph(new Run(left))
            {
                Margin = new Thickness(0),
                Inlines = {
            new InlineUIContainer(new TextBlock {
                Text = right,
                Width = 270 - (left.Length * 7), // Rough estimate for Consolas
                TextAlignment = TextAlignment.Right
            })
        }
            };
        }

        private TableCell Cell(string text,
                       TextAlignment alignment = TextAlignment.Left,
                       FontWeight? fontWeight = null)
        {
            var p = new Paragraph(new Run(text)) { Margin = new Thickness(0) };
            if (fontWeight.HasValue) p.FontWeight = fontWeight.Value;

            return new TableCell(p)
            {
                TextAlignment = alignment,
                Padding = new Thickness(0, 2, 0, 2)
            };
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
    }
}
