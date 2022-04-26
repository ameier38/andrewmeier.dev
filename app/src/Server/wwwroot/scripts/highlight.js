function waitForPrism() {
   if (typeof Prism !== 'undefined') {
       var el = document.getElementById("post")
       Prism.highlightAllUnder(el)
   } else {
       setTimeout(waitForPrism, 250) 
   }
}
waitForPrism()
