<template>
  <div class="wishlist-page min-h-screen flex flex-col bg-gray-900 text-[#C8B280]">
    <NavBar />

    <!-- ✅ Toasts -->
    <Toast :toasts="toasts" @close="removeToast" />

    <main class="flex-1 container mx-auto px-4 py-6">
      <h1 class="text-3xl font-bold text-[#F5DEB3] mb-6">Your Wishlist</h1>

      <!-- Empty State -->
      <div v-if="!wishlistItems.length" class="flex flex-col items-center justify-center text-[#aaa] py-20">
        <p class="text-lg mb-4">Your wishlist is currently empty.</p>
        <router-link to="/shop" class="px-6 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition">
          Browse Books
        </router-link>
      </div>

      <!-- Wishlist Items -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <transition-group name="fade" tag="div" class="contents">
          <div
              v-for="item in wishlistItems"
              :key="item.id"
              class="bg-[#1a1a1a] rounded-xl p-5 flex flex-col shadow border border-[#333] hover:shadow-md transition"
          >
            <div class="relative">
              <img
                  :src="normalizeImage(item.images[0])"
                  :alt="item.title"
                  class="w-full h-48 object-cover rounded mb-4"
              />
              <button
                  @click="removeFromWishlist(item)"
                  class="absolute top-2 right-2 w-8 h-8 bg-[#333] bg-opacity-70 rounded-full flex items-center justify-center hover:bg-red-600 transition"
              >
                <i class="fas fa-times text-white"></i>
              </button>
            </div>

            <div class="flex-1">
              <h2 class="text-xl font-semibold truncate text-[#F5DEB3]">{{ item.title }}</h2>
              <p class="text-sm text-[#aaa] line-clamp-2 my-2">{{ item.description }}</p>
              <p class="text-sm text-[#C8B280] mb-4">
                Price: <span class="font-bold">${{ item.price.toFixed(2) }}</span>
              </p>
            </div>

            <div class="mt-auto flex justify-between items-center">
              <button
                  @click="handleAddToCart(item)"
                  class="px-4 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition flex-1 mr-2"
                  :disabled="loadingCart"
              >
                <i class="fas fa-shopping-cart mr-2"></i>
                {{ loadingCart ? 'Adding...' : 'Add to Cart' }}
              </button>
              <button
                  @click="shareItem(item)"
                  class="w-10 h-10 bg-[#333] rounded flex items-center justify-center hover:bg-[#444] transition"
              >
                <i class="fas fa-share-alt text-[#C8B280]"></i>
              </button>
            </div>
          </div>
        </transition-group>
      </div>

      <!-- Action Buttons -->
      <div v-if="wishlistItems.length" class="mt-8 flex flex-col md:flex-row justify-center space-y-4 md:space-y-0 md:space-x-4">
        <button
            @click="addAllToCart"
            class="px-6 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition"
            :disabled="loadingCart"
        >
          {{ loadingCart ? 'Processing...' : 'Add All to Cart' }}
        </button>
        <button
            @click="clearWishlist"
            class="px-6 py-2 bg-[#8B0000] text-white rounded hover:bg-[#a30000] transition"
        >
          Clear Wishlist
        </button>
      </div>
    </main>

    <!-- Share Modal -->
    <!-- [Unchanged modal code here...] -->

    <FooterComponent />
  </div>
</template>

<script setup>
import { ref, onMounted, reactive } from 'vue'
import axios from 'axios'
import NavBar from '@/components/NavBar.vue'
import FooterComponent from '@/components/Footer.vue'
import Toast from '@/components/Toast.vue' // ✅ Import Toast
import { useCart } from '@/store/useCart'

const { addToCart } = useCart()

const baseUrl = process.env.VUE_APP_API_URL || window.location.origin

const wishlistItems = ref([])
const isLoggedIn = ref(false)
const showShareModal = ref(false)
const currentShareItem = ref(null)
const shareUrl = ref('')
const toasts = reactive([])
const loadingCart = ref(false)

function showToast(message, type = 'success') {
  const id = Date.now() + Math.random()
  toasts.push({ id, message, type })
  setTimeout(() => removeToast(id), 5000)
}

// ✅ Close toast
function removeToast(id) {
  const index = toasts.findIndex(t => t.id === id)
  if (index !== -1) toasts.splice(index, 1)
}

function normalizeImage(path) {
  if (!path) return '/fallback.png'
  if (/^https?:\/\//.test(path)) return path
  return `${baseUrl.replace(/\/$/, '')}${path.startsWith('/') ? '' : '/'}${path}`
}

function authHeader() {
  const token = localStorage.getItem('authToken')
  return token ? { Authorization: `Bearer ${token}` } : {}
}

async function fetchWishlist() {
  try {
    const res = await axios.get(`${baseUrl}/api/bookmarks`, {
      headers: authHeader()
    })
    wishlistItems.value = res.data.data.map(b => ({
      id: b.bookId,
      title: b.title,
      description: b.description,
      price: b.price,
      images: b.images
    }))
  } catch {
    showToast('Failed to load wishlist', 'error')
  }
}

async function handleAddToCart(item) {
  loadingCart.value = true
  const result = await addToCart(item, 1)
  showToast(result.message, result.success ? 'success' : 'error')
  loadingCart.value = false
}

async function addAllToCart() {
  loadingCart.value = true
  const failures = []
  for (const item of wishlistItems.value) {
    const result = await addToCart(item, 1)
    if (!result.success) failures.push(item.title)
  }
  showToast(
      failures.length ? `Failed to add: ${failures.join(', ')}` : 'All items added to cart',
      failures.length ? 'error' : 'success'
  )
  loadingCart.value = false
}

async function removeFromWishlist(item) {
  try {
    await axios.delete(`${baseUrl}/api/bookmarks/${item.id}`, {
      headers: authHeader()
    })
    wishlistItems.value = wishlistItems.value.filter(i => i.id !== item.id)
    showToast('Removed from wishlist')
  } catch {
    showToast('Failed to remove item', 'error')
  }
}

async function clearWishlist() {
  try {
    await Promise.all(wishlistItems.value.map(item =>
        axios.delete(`${baseUrl}/api/bookmarks/${item.id}`, {
          headers: authHeader()
        })
    ))
    wishlistItems.value = []
    showToast('Wishlist cleared')
  } catch {
    showToast('Failed to clear wishlist', 'error')
  }
}

function shareItem(item) {
  currentShareItem.value = item
  shareUrl.value = `${window.location.origin}/shop/book/${item.id}`
  showShareModal.value = true
}

function copyToClipboard() {
  navigator.clipboard.writeText(shareUrl.value)
      .then(() => showToast('Copied'))
      .catch(() => showToast('Copy failed', 'error'))
}

function shareOnSocial(platform) {
  const text = `Check out ${currentShareItem.value.title}`
  let url = ''
  switch (platform) {
    case 'facebook':
      url = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(shareUrl.value)}`
      break
    case 'twitter':
      url = `https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(shareUrl.value)}`
      break
    case 'whatsapp':
      url = `https://wa.me/?text=${encodeURIComponent(text + ' ' + shareUrl.value)}`
      break
  }
  window.open(url, '_blank')
  showShareModal.value = false
}

onMounted(() => {
  isLoggedIn.value = !!localStorage.getItem('authToken')
  if (isLoggedIn.value) fetchWishlist()
})
</script>

<style scoped>
.line-clamp-2 {
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}
.fade-enter-active, .fade-leave-active {
  transition: all 0.3s ease;
}
.fade-enter-from, .fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}
.contents {
  display: contents;
}
</style>
