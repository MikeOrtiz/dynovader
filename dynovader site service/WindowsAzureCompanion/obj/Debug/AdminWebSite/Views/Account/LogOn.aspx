<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<WindowsAzureCompanion.AdminWebSite.Models.LogOnModel>" %>

<asp:Content ID="loginTitle" ContentPlaceHolderID="ContentPlaceHolder_Title" runat="server">
    Log On
</asp:Content>

<asp:Content ID="loginContent" ContentPlaceHolderID="ContentPlaceHolder_Body" runat="server">
    <div class="wraper">
        <div class="pagetilte">Log In</div>
            <% using (Html.BeginForm()) { %>
                   <div class="grey-border-alt">
                        <div class="table-head">Account Information</div>
                        <div class="tabPadding">
                        <%= Html.ValidationSummary(true, "Login was unsuccessful. Please correct the errors and try again.") %>
                        <div class="editor-label">
                            <%= Html.LabelFor(m => m.UserName) %>
                        </div>
                        <div class="editor-field">
                            <%= Html.TextBoxFor(m => m.UserName, new {size = 20}) %>
                            <%= Html.ValidationMessageFor(m => m.UserName) %>
                        </div>
                        <br/>
                
                        <div class="editor-label">
                            <%= Html.LabelFor(m => m.Password) %>
                        </div>
                        <div class="editor-field">
                            <%= Html.PasswordFor(m => m.Password, new {size = 21}) %>
                            <%= Html.ValidationMessageFor(m => m.Password) %>
                        </div>
                        <br/>
                
                        <div class="editor-label">
                            <%= Html.CheckBoxFor(m => m.RememberMe) %>
                            <%= Html.LabelFor(m => m.RememberMe) %>
                        </div>
                
                        <div class="editor-submit">
                            <br/>
                            <input type="submit" value="Log On" class="btnOrange" />
                        </div>
                        <br/>
                    </div>
                </div>
            <% } %>
        </div>
    </div>
</asp:Content>