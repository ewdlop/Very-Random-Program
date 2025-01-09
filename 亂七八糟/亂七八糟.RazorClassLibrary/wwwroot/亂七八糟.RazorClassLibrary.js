window.scrollToBottom = (element) => {
    element .scrollTop = element.scrollHeight;
}

window.markdownToHtml = (markdown) => {
    return marked.parse(markdown);
}