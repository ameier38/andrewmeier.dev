import prism from 'prismjs'
import 'prismjs/components/prism-markup'
import 'prismjs/components/prism-fsharp'
import 'prismjs/components/prism-python'
import 'prismjs/components/prism-go'
import 'prismjs/components/prism-shell-session'
import 'prismjs/components/prism-bash'
import 'prismjs/components/prism-powershell'
import 'prismjs/components/prism-json'
import '../styles/prism.css'
import '../styles/post.css'

export const highlightAllUnder = ref => {
    prism.highlightAllUnder(ref)
}
