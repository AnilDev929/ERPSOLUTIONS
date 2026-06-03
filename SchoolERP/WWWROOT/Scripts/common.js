
/* =========================
              INPUT HELPERS
==========================*/

window.allowOnlyNumbers = function (input) {
    $(input).val($(input).val().replace(/\D/g, ''));
};

window.allowOnlyLetters = function (evt) {
    let char = String.fromCharCode(evt.which);
    if (!/^[a-zA-Z\s]$/.test(char)) {
        evt.preventDefault();
    }
};


window.cleanLetters = function (input) {
    $(input).val($(input).val().replace(/[^a-zA-Z\s]/g, ''));
};

/* =========================
    CURRENCY
==========================*/
window.formatCurrency = function (value) {
    return value.toLocaleString('en-IN', {
        style: 'currency',
        currency: 'INR'
    });
};

window.cleanLetters = function (input) {
    $(input).val($(input).val().replace(/[^a-zA-Z\s]/g, ''));
};

window.preventNegative = function (input) {
    let value = parseFloat(input.value);
    if (!isNaN(value) && value < 0) input.value = 0;
};


window.validateMonths = function () {
    let el = $("#Months");
    let value = parseInt(el.val()) || 0;

    if (value < 0) value = 0;
    if (value > 12) value = 12;

    el.val(value);
};