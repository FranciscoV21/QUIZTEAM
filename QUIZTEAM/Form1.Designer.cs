namespace QUIZTEAM
{
    partial class Form1
    {
        /// <summary>
        /// Variable del diseñador necesaria.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén usando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben desechar; false en caso contrario.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de Windows Forms

        /// <summary>
        /// Método necesario para admitir el Diseñador. No se puede modificar
        /// el contenido de este método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.BotComenzar = new System.Windows.Forms.Button();
            this.BotSalir = new System.Windows.Forms.Button();
            this.ImagBienvenida = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.ImagBienvenida)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 35F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(1067, 469);
            this.textBox1.TabIndex = 0;
            this.textBox1.Text = "BIENVENIDO A NUESTRO QUIZ";
            this.textBox1.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            // 
            // BotComenzar
            // 
            this.BotComenzar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BotComenzar.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BotComenzar.Location = new System.Drawing.Point(0, 364);
            this.BotComenzar.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BotComenzar.Name = "BotComenzar";
            this.BotComenzar.Size = new System.Drawing.Size(1067, 105);
            this.BotComenzar.TabIndex = 1;
            this.BotComenzar.Text = "CLICK AQUI PARA COMENZAR";
            this.BotComenzar.UseVisualStyleBackColor = true;
            this.BotComenzar.Click += new System.EventHandler(this.BotComenzar_Click);
            // 
            // BotSalir
            // 
            this.BotSalir.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BotSalir.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BotSalir.Location = new System.Drawing.Point(0, 469);
            this.BotSalir.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.BotSalir.Name = "BotSalir";
            this.BotSalir.Size = new System.Drawing.Size(1067, 85);
            this.BotSalir.TabIndex = 2;
            this.BotSalir.Text = "SALIR";
            this.BotSalir.UseVisualStyleBackColor = true;
            this.BotSalir.Click += new System.EventHandler(this.BotSalir_Click);
            // 
            // ImagBienvenida
            // 
            this.ImagBienvenida.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.ImagBienvenida.Location = new System.Drawing.Point(0, 117);
            this.ImagBienvenida.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ImagBienvenida.Name = "ImagBienvenida";
            this.ImagBienvenida.Size = new System.Drawing.Size(1067, 247);
            this.ImagBienvenida.TabIndex = 3;
            this.ImagBienvenida.TabStop = false;
            this.ImagBienvenida.Click += new System.EventHandler(this.ImagBienvenida_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1067, 554);
            this.Controls.Add(this.ImagBienvenida);
            this.Controls.Add(this.BotComenzar);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.BotSalir);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "Form1";
            this.Text = "Form1";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.ImagBienvenida)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button BotComenzar;
        private System.Windows.Forms.Button BotSalir;
        private System.Windows.Forms.PictureBox ImagBienvenida;
    }
}

