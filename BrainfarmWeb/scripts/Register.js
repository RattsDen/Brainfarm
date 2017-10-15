/*
Scripts for providing interaction with the account registration page
*/

$(document).ready(function () {

    /*$(document).on("click", "[id$=btnRegister]", function (event) {
        if (!(passwordFieldsMatch() && passwordMeetsRequirements())) {
            event.preventDefault();
            window.alert("NO");
        }
    });*/

});

function passwordFieldsMatch() {
    return $("[id$=txtNewPassword]").val() == $("[id$=txtPasswordConfirm]").val()
}

function passwordMeetsRequirements() {
    return $("[id$=txtNewPassword]").val().length >= 8;
}
