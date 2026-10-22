using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace WinFormsAnchored
{
    [ToolboxItem(true)]
    [DefaultProperty("CardBackColor")]
    public class CardPanel : Panel
    {
        private int _borderRadius = 15;
        private Color _borderColor = Color.FromArgb(220, 224, 230);
        private int _borderSize = 1;
        private Color _backgroundColor = Color.White;

        [Category("Material Design")]
        [Description("O raio de arredondamento dos cantos do Card.")]
        public int BorderRadius
        {
            get => _borderRadius;
            set { _borderRadius = Math.Max(0, value); this.Invalidate(); }
        }

        [Category("Material Design")]
        [Description("A cor da borda personalizada do Card.")]
        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; this.Invalidate(); }
        }

        [Category("Material Design")]
        [Description("A espessura da borda do Card (Defina como 0 para remover a borda).")]
        public int BorderSize
        {
            get => _borderSize;
            set { _borderSize = Math.Max(0, value); this.Invalidate(); }
        }

        [Category("Material Design")]
        [Description("A cor de fundo interna do Card.")]
        public Color CardBackColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; this.Invalidate(); }
        }

        public CardPanel()
        {
            // Ativa propriedades de renderização avançada e buffer duplo para evitar piscadas
            this.SetStyle(ControlStyles.UserPaint |
                          ControlStyles.AllPaintingInWmPaint |
                          ControlStyles.OptimizedDoubleBuffer |
                          ControlStyles.ResizeRedraw, true);

            this.BackColor = Color.Transparent;
            this.Size = new Size(250, 150);
        }

        // Método geométrico otimizado para curvas matemáticas perfeitas
        private static GraphicsPath GetRoundedPath(RectangleF rect, float radius)
        {
            GraphicsPath path = new();
            float diameter = radius * 2F;

            path.StartFigure();
            // Canto Superior Esquerdo
            path.AddArc(rect.X, rect.Y, diameter, diameter, 180, 90);
            // Canto Superior Direito
            path.AddArc(rect.Right - diameter, rect.Y, diameter, diameter, 270, 90);
            // Canto Inferior Direito
            path.AddArc(rect.Right - diameter, rect.Bottom - diameter, diameter, diameter, 0, 90);
            // Canto Inferior Esquerdo
            path.AddArc(rect.X, rect.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();

            return path;
        }

        /*
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;

            // --- BLINDAGEM CONTRA SERRILHAMENTO (ALTA QUALIDADE) ---
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Trabalhar com RectangleF (ponto flutuante) evita arredondamentos incorretos de pixel do tipo int
            RectangleF rectSurface = new RectangleF(0, 0, this.Width, this.Height);

            // O segredo do recuo: reduzimos exatamente a metade do tamanho da borda para que ela assente perfeitamente
            float insetOffset = (float)_borderSize / 2F;
            RectangleF rectBorder = new RectangleF(insetOffset, insetOffset, this.Width - _borderSize, this.Height - _borderSize);

            if (_borderRadius > 2)
            {
                using (GraphicsPath pathSurface = GetRoundedPath(rectSurface, _borderRadius))
                using (GraphicsPath pathBorder = GetRoundedPath(rectBorder, _borderRadius - insetOffset))
                using (SolidBrush brushBackground = new SolidBrush(_backgroundColor))
                using (Pen penBorder = new Pen(_borderColor, _borderSize))
                {
                    // Força o alinhamento da linha para dentro do caminho demarcado
                    penBorder.Alignment = PenAlignment.Inset;

                    // 1. Recorta a região do painel para os controles filhos não vazarem
                    this.Region = new Region(pathSurface);

                    // 2. Limpa o fundo desenhando a superfície interna limpa
                    graphics.FillPath(brushBackground, pathSurface);

                    // 3. Desenha o contorno da borda com precisão milimétrica
                    if (_borderSize > 0)
                    {
                        graphics.DrawPath(penBorder, pathBorder);
                    }
                }
            }
            else // Modo quadrado tradicional sem arredondamento
            {
                this.Region = new Region(rectSurface);
                using (SolidBrush brushBackground = new SolidBrush(_backgroundColor))
                using (Pen penBorder = new Pen(_borderColor, _borderSize))
                {
                    penBorder.Alignment = PenAlignment.Inset;
                    graphics.FillRectangle(brushBackground, rectSurface);
                    if (_borderSize > 0)
                    {
                        graphics.DrawRectangle(penBorder, 0, 0, this.Width, this.Height);
                    }
                }
            }
        }
        */

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics graphics = e.Graphics;

            // Configurações máximas de renderização e suavização
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

            // Retângulo limpo em formato int (evita distorções de arredondamento de float nas bordas do controle)
            Rectangle rectSurface = new Rectangle(0, 0, this.Width, this.Height);

            if (_borderRadius > 2)
            {
                using (GraphicsPath pathSurface = GetRoundedPath(new RectangleF(0, 0, this.Width, this.Height), _borderRadius))
                using (SolidBrush brushBackground = new SolidBrush(_backgroundColor))
                {
                    // 1. O GRANDE SEGREDO: Primeiro, dizemos ao Windows Forms para desenhar o fundo nativo do controle pai.
                    // Isso preenche as quinas externas com a cor exata do formulário antes de desenharmos o card,
                    // impedindo a criação de vácuos ou pontos pretos de transparência.
                    this.InvokePaintBackground(this, e);

                    // 2. Aplica o recorte físico exato da Região. 
                    // Agora os controles internos ficam perfeitamente presos dentro da curva, sem rebarbas.
                    this.Region = new Region(pathSurface);

                    // 3. Preenche a área do Card com a cor de fundo interna (Ex: Branco)
                    graphics.FillPath(brushBackground, pathSurface);

                    // 4. Desenha a borda sutil por dentro se a espessura for maior que zero
                    if (_borderSize > 0)
                    {
                        // Usamos PenAlignment.Inset para garantir que a linha vá estritamente para DENTRO
                        // e fique colada no limite interno do card, sem vazar para a zona de corte da região.
                        using (Pen penBorder = new Pen(_borderColor, _borderSize))
                        {
                            penBorder.Alignment = PenAlignment.Inset;
                            graphics.DrawPath(penBorder, pathSurface);
                        }
                    }
                }
            }
            else // Modo quadrado convencional 100% seguro
            {
                this.Region = new Region(rectSurface);
                using (SolidBrush brushBackground = new SolidBrush(_backgroundColor))
                using (Pen penBorder = new Pen(_borderColor, _borderSize))
                {
                    penBorder.Alignment = PenAlignment.Inset;
                    graphics.FillRectangle(brushBackground, rectSurface);
                    if (_borderSize > 0)
                    {
                        graphics.DrawRectangle(penBorder, 0, 0, this.Width - 1, this.Height - 1);
                    }
                }
            }
        }

        protected override void OnResize(EventArgs eventargs)
        {
            base.OnResize(eventargs);
            this.Invalidate(); // Repinta imediatamente ao redimensionar garantindo curvas esticadas sem rastro
        }
    }
}