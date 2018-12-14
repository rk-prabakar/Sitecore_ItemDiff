//https://www.w3schools.com/js/tryit.asp?filename=tryjs_addeventlistener_displaydate

function hideToolbar()
{
document.getElementsByClassName("scToolbar")[0].style.visibility = 'hidden';
}

Event.observe(window, "load", hideToolbar);