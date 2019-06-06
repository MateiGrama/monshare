def levenshtein(s, t):
    rows = len(s) + 1
    cols = len(t) + 1
    dist = [[0 for _ in range(cols)] for _ in range(rows)]

    for i in range(1, rows):
        dist[i][0] = i
    for i in range(1, cols):
        dist[0][i] = i

    for col in range(1, cols):
        for row in range(1, rows):
            if s[row - 1] == t[col - 1]:
                cost = 0
            else:
                cost = 1
            dist[row][col] = min(dist[row - 1][col] + 1,         # deletion
                                 dist[row][col - 1] + 1,         # insertion
                                 dist[row - 1][col - 1] + cost)  # substitution

    return dist[rows - 1][cols - 1]


class TrieNode:
    def __init__(self):
        self.children = {}
        self.last = False


class Trie:
    def __init__(self, keys):
        self.root = TrieNode()
        self.word_list = []
        for key in keys:
            self.insert(key)

    def insert(self, key):
        node = self.root

        for a in list(key):
            if not node.children.get(a):
                node.children[a] = TrieNode()

            node = node.children[a]

        node.last = True

    def search(self, key):
        node = self.root
        found = True

        for a in list(key):
            if not node.children.get(a):
                found = False
                break

            node = node.children[a]

        return node and node.last and found

    def suggestions_rec(self, node, word):
        if node.last:
            self.word_list.append(word)

        for a, n in node.children.items():
            self.suggestions_rec(n, word + a)

    def get_auto_suggestions(self, key):
        node = self.root
        not_found = False
        temp_word = ''

        for a in list(key):
            if not node.children.get(a):
                not_found = True
                break

            temp_word += a
            node = node.children[a]

        if not_found:
            return []

        self.suggestions_rec(node, temp_word)
        return self.word_list
