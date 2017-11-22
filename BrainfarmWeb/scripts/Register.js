$(document).ready(function () {

    /*$(document).on("click", "[id$=btnRegister]", function (event) {
        if (!(passwordFieldsMatch() && passwordMeetsRequirements())) {
            event.preventDefault();
            window.alert("NO");
        }
    });*/

    $("[id$=txtPasswordConfirm]").keyup(comparePasswordsAndDisableButton);
    $("[id$=txtNewPassword]").keyup(comparePasswordsAndDisableButton);

});

function comparePasswordsAndDisableButton() {
    var btnRegister = $("[id$=btnRegister]");
    var btnChangePassword = $("[id$=btnChangePassword]");
    var lblError = $("[id$=lblError]");
    if (!passwordFieldsMatch()) {
        btnChangePassword.attr("disabled", "disabled");
        btnRegister.attr("disabled", "disabled");
        lblError.text("Passwords do not match");
        lblError.show();
    }
    else {
        btnChangePassword.attr("disabled", false);
        btnRegister.attr("disabled", false);
        lblError.hide();
    }
}

function passwordFieldsMatch() {
    return $("[id$=txtNewPassword]").val() == $("[id$=txtPasswordConfirm]").val()
}

function passwordMeetsRequirements() {
    return $("[id$=txtNewPassword]").val().length >= 8;
}
