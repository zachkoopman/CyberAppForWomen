<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Welcome_Page.aspx.cs" Inherits="CyberApp_FIA.Welcome_Page" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>FIA • Home</title>
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <!-- Google Fonts -->
    <link href="https://fonts.googleapis.com/css2?family=Lato:wght@300;400&family=Poppins:wght@500;700&display=swap" rel="stylesheet" />
    <style>
        :root{
            /* Brand Palette */
            --fia-pink:#f06aa9;
            --fia-blue:#2a99db;
            --fia-teal:#45c3b3;
            --ink:#1c1c1c;
            --muted:#6b7280;
            --bg:#ffffff;
            --chip:#f6f7fb;
            --ring:rgba(42,153,219,.25);
        }

        *{box-sizing:border-box}
        html,body{height:100%}
        body{
            margin:0;
            font-family:"Lato", system-ui, -apple-system, Segoe UI, Roboto, Arial, sans-serif;
            color:var(--ink);
            background:linear-gradient(135deg,#ffffff 0%, #f9fbff 100%);
        }

        /* Header */
        .site-header{
            display:flex;
            align-items:center;
            justify-content:space-between;
            gap:1rem;
            padding:24px clamp(16px,4vw,48px);
        }
        .brand{
            display:flex;
            align-items:center;
            gap:12px;
        }
        .brand-badge{
            width:44px;height:44px;border-radius:12px;
            display:grid;place-items:center;
            background:linear-gradient(135deg,var(--fia-pink),var(--fia-blue));
            color:#fff;font-weight:700;font-family:"Poppins", sans-serif;
            letter-spacing:.5px;
        }
        .brand h1{
            margin:0;font-family:"Poppins", sans-serif;font-weight:700;
            font-size:1.25rem;line-height:1.1;
        }
        .nav-chips{
            display:flex;gap:8px;flex-wrap:wrap;
        }
        .chip{
            padding:8px 12px;border-radius:999px;background:var(--chip);
            color:#455; font-size:.9rem;
            border:1px solid #e7eaf3;
        }

        /* Hero */
        .hero{
            padding:clamp(24px,6vw,72px) clamp(16px,4vw,48px);
            display:grid;
            grid-template-columns:1.1fr .9fr;
            gap:clamp(20px,4vw,48px);
            align-items:center;
        }
        @media (max-width:900px){ .hero{ grid-template-columns:1fr } }

        .hero h2{
            font-family:"Poppins", sans-serif;
            font-weight:600;
            font-size:clamp(1.8rem,3.2vw,2.6rem);
            margin:0 0 12px 0;
        }
        .hero p{color:var(--muted); font-size:1.05rem; margin:0 0 20px 0;}
        .accent{
            color:var(--fia-blue);
        }
        .tagline{
            display:inline-block;
            padding:6px 12px;
            margin-bottom:14px;
            border-radius:999px;
            background:rgba(240,106,169,.1);
            color:var(--fia-pink);
            font-weight:600;
            font-family:"Poppins", sans-serif;
            letter-spacing:.3px;
        }

        /* Image grid placeholders */
        .grid{
            display:grid;
            grid-template-columns:repeat(6,1fr);
            grid-auto-rows:120px;
            gap:12px;
        }
        .tile{
            border-radius:16px;
            background:#fff;
            border:1px solid #eef1f6;
            box-shadow:0 6px 20px rgba(0,0,0,.04);
            position:relative; overflow:hidden;
        }
        /* Hide helper label once images are used */
        .tile::after{ content:none; }

        /* Make images centered and “zoomed out” inside tiles */
        .tile-img{
            width:100%;
            height:100%;
            object-fit:contain;      /* show whole photo (zoomed out) */
            object-position:center;  /* keep centered */
            display:block;
            background:#fff;         /* prevents odd transparency edges */
        }

        .t1{ grid-column:span 3; }
        .t2{ grid-column:span 3; }
        .t3{ grid-column:span 2; }
        .t4{ grid-column:span 2; }
        .t5{ grid-column:span 2; }

        /* CTA buttons */
        .cta{
            padding:0  clamp(16px,4vw,48px)  clamp(28px,6vw,72px);
        }
        .panel{
            border-radius:20px;
            background:#fff;
            border:1px solid #e8eef7;
            box-shadow:0 10px 30px rgba(42,153,219,.08);
            padding:24px;
            display:flex; flex-wrap:wrap; align-items:center; justify-content:space-between; gap:16px;
        }
        .panel h3{
            font-family:"Poppins", sans-serif; margin:0 0 6px 0; font-size:1.25rem;
        }
        .panel p{margin:0; color:var(--muted)}
        .btnrow{ display:flex; gap:12px; flex-wrap:wrap; }
        .btn{
            appearance:none; border:none; cursor:pointer; font-weight:700;
            font-family:"Poppins", sans-serif;
            padding:12px 18px; border-radius:12px; transition:.15s ease transform, box-shadow;
            outline:2px solid transparent; outline-offset:2px;
        }
        .btn:focus{ box-shadow:0 0 0 6px var(--ring); }
        .btn-primary{
            background:linear-gradient(135deg,var(--fia-blue),var(--fia-teal));
            color:#fff;
        }
        .btn-outline{
            background:#fff; color:var(--fia-blue); border:2px solid var(--fia-blue);
        }
        .btn:hover{ transform:translateY(-1px) }
        .btn:active{ transform:translateY(0) }

        /* Footer */
        .site-footer{
            padding:20px clamp(16px,4vw,48px);
            color:#7a8796; font-size:.9rem;
            display:flex; justify-content:space-between; flex-wrap:wrap; gap:8px;
            border-top:1px solid #eef1f6;
        }
        .dots{
            display:flex; gap:8px; align-items:center;
        }
        .dot{ width:14px; height:14px; border-radius:50% }
        .c1{ background:#404040 } .c2{ background:#d9d9d9 } .c3{ background:#ff2b12 }
        .c4{ background:#ff7a1a } .c5{ background:#b1c12a } .c6{ background:#56c7b5 }
        .c7{ background:#2c82b7 } .c8{ background:#a55bd7 } .c9{ background:#ff6fbd }
    </style>
</head>
<body>
    <form id="form1" runat="server">
        <!-- Header -->
        <header class="site-header" role="banner">
            <div class="brand">
                <div class="brand-badge">FIA</div>
                <h1>Feminine Intelligence Agency</h1>
            </div>
            <nav class="nav-chips" aria-label="Quick links">
                <span class="chip">About Us</span>
                <span class="chip">Our Team</span>
                <span class="chip">Contact Us</span>
            </nav>
        </header>

        <!-- Hero -->
        <section class="hero" role="main">
            <div>
                <span class="tagline">Feminine Intelligence Agency</span>
                <h2>Welcome to <span class="accent">Cybersecurity App For Women!</span></h2>
                <p>
                    This Feminine Intelligence Agency (FIA) application is a trauma-informed cybersecurity app that helps women 
                    learn practical digital-safety skills through short, mentor-guided “booths.” It pairs clear, bite-sized 
                    lessons with supportive community features so users can practice, track progress, and stay safe online
                </p>
                <p>
                    Click with confidence: learn, practice, and stay safe online. We hope you enjoy the application!
                </p>
            </div>

            <!-- Image placeholders -->
            <div class="grid" aria-label="Image showcase">
                <div class="tile t1"> <asp:Image runat="server" CssClass="tile-img" ImageUrl="~/VisualContent/kissing2.jpeg" AlternateText="Image 1" /> </div>
                <div class="tile t2"> <asp:Image runat="server" CssClass="tile-img" ImageUrl="~/VisualContent/weirdos3.jpeg" AlternateText="Image 2" /> </div>
                <div class="tile t3"> <asp:Image runat="server" CssClass="tile-img" ImageUrl="~/VisualContent/latinaPaintWall.jpeg" AlternateText="Image 3" /> </div>
                <div class="tile t4"> <asp:Image runat="server" CssClass="tile-img" ImageUrl="~/VisualContent/gogglesOrange.jpeg" AlternateText="Image 4" /> </div>
                <div class="tile t5"> <asp:Image runat="server" CssClass="tile-img" ImageUrl="~/VisualContent/diamondShielf.jpeg" AlternateText="Image 5" /> </div>
            </div>
        </section>

        <!-- Call to Action -->
        <section class="cta" aria-label="Account actions">
            <div class="panel">
                <div>
                    <h3>Get started</h3>
                    <p>Create your account or sign in to continue.</p>
                </div>
                <div class="btnrow">
                    <!-- Point these PostBackUrl values at your actual routes/pages -->
                    <asp:Button ID="BtnCreate" runat="server" Text="Create Account" CssClass="btn btn-primary"
                        PostBackUrl="~/Account/CreateAccountPage.aspx" OnClick="BtnCreate_Click" />
                    <asp:Button ID="BtnSignIn" runat="server" Text="Sign In" CssClass="btn btn-outline"
                        PostBackUrl="~/Account/Login.aspx" />
                </div>
            </div>
        </section>

        <!-- Footer -->
        <footer class="site-footer">
            <div>© <%= DateTime.Now.Year %> Feminine Intelligence Agency</div>
            <div class="dots" aria-hidden="true">
                <div class="dot c1"></div><div class="dot c2"></div><div class="dot c3"></div>
                <div class="dot c4"></div><div class="dot c5"></div><div class="dot c6"></div>
                <div class="dot c7"></div><div class="dot c8"></div><div class="dot c9"></div>
            </div>
        </footer>
    </form>
</body>
</html>

