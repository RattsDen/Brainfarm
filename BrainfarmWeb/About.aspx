<%@ Page Title="" Language="C#" MasterPageFile="~/Layout.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="BrainfarmWeb.About" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    
    <link rel="stylesheet" href="/styles/About.css"/>

</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="BodyContentPlaceHolder" runat="server">

    <div class="panel">

        <h2>What is Brainfarm all About?</h2>

        <h3>Developing Ideas for Projects, and Motivating Builder People to create stuff!</h3>

        <p>
            Sometimes people good at building things need ideas for projects to build for...
        </p>

        <p>
            [two small cartoon pictures horizontally side-by-side:]<br />
            [picture of smiling cartoon artist guy at easel]<br />
            [picture of smiling cartoon coder gal in front of computer with &lt;code&gt; &lt;code&gt; on the screen. 
            She has a speech bubble: "I want to practice my coding skills! I'd be more motivated if I had some small project to contribute to!"]
        </p>

        <p>
            ... and some people have lots of ideas, but not the skills to build stuff.
        </p>

        <p>
            [cartoon picture of someone in a thinking position in a hammock, with a thought bubble; the thought bubble says: "I have so much knowledge about existentialism. I would love to put this into a videogame, but I don't have any coding skills..."]
        </p>


        <h3>Enter Brainfarm</h3>

        <p>
            Brainfarm is a reddit-style discussion forum, with a handful of extra features. 
            The goal: to help idea people to develop ideas, and to motivate builder people 
            to build stuff for those ideas!
        </p>


        <h3>Three special kinds of comments</h3>

        <p>
            Let's learn about the features! Here's an example of a discussion:
        </p>

        <div class="layout-row">
            <img src="/img/about1.png" />
            <p>
                A person will start a new page by posting a new 
                <span class="highlight-project">PROJECT</span>, 
                with an initial idea for the project.
                <br />
                Lots of discussion happens. But the best ideas might be scattered!
            </p>
        </div>

        <div class="layout-row">
            <img src="/img/about2.png" />
            <div>
                <p>
                    Anyone can make a <span class="highlight-synth">SYNTHESIS</span> comment 
                    to gather what they think are the best ideas, all into one reply. They 
                    select the comments with the best ideas, and links to those comments appear!
                </p>
                <p>
                    Some builder people are motivated by detailed specifications to respond to. 
                    <span class="highlight-spec">SPECIFICATION</span> comments are like a 
                    challenge to builder people.
                </p>
                <p>
                    When a person makes a contribution - a photo of some art, or some source 
                    code, for example, they can upload it in a 
                    <span class="highlight-contrib">CONTRIBUTION</span> comment.
                </p>
            </div>
        </div>


        <h3>Other features</h3>

        <p>
            Any user can <span class="highlight">bookmark</span> their favourite comments on 
            a project. This helps that user find their favourite comments easier.
        </p>

        <div class="layout-row">
            <p>
                All comments can be <span class="highlight">filtered</span> so that only certain 
                kinds of comments show on a page. It's easy, for example, to show only the 
                comments you have bookmarked, or only specification comments (for example, 
                if you're a builder looking for challenges).
            </p>
            <img src="/img/about3.png" />
        </div>

        <div class="layout-row">
            <p>
                The person who creates a project gives the projects some 
                <span class="highlight">tags</span>. From the home page, projects can be 
                searched by title or by tags.
            </p>
            <img src="/img/about4.png" />
        </div>


        <h3>Why not go ahead and try it?</h3>

        <p>
            Why not participate in a project discussion? The project idea will develop 
            more and more, and builder people will be motivated to build stuff.
        </p>

        <p>
            The three special comments will help all of this discussion to happen; The 
            best ideas will be synthesized with <span class="highlight-synth">SYNTHESIS</span> 
            comments, and builders will look for <span class="highlight-spec">SPECIFICATION</span> 
            comments and showcase their work with <span class="highlight-contrib">CONTRIBUTION</span> 
            comments.
        </p>

        <p>
            You can go to the <a href="/Default.aspx">home page</a> to see some popular projects, 
            <a href="/SearchForProjects.aspx">search for projects</a> that you might be interested 
            in, or perhaps you would like to <a href="/Register.aspx">create a new account</a> 
            (or <a href="#div-login">login</a> if you've already made one) so you can join some 
            project's discussion!
        </p>

    </div>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="SidebarContentPlaceHolder" runat="server">
</asp:Content>
