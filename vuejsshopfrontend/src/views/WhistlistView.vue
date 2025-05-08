<template>
  <div class="wishlist-page min-h-screen flex flex-col bg-[#0f0f0f] text-[#C8B280]">
    <!-- NavBar Component -->
    <nav class="bg-[#23110d] py-4 shadow-md sticky top-0 z-10 text-[#C8B280]">
      <!-- Toasts -->
      <div class="toast-container fixed top-4 right-4 z-50 w-80">
        <div
            v-for="toast in toasts"
            :key="toast.id"
            :class="['p-3 mb-3 rounded shadow-lg flex items-center justify-between transition-all transform translate-x-0',
                  toast.type === 'success' ? 'bg-green-800 text-white' : 
                  toast.type === 'error' ? 'bg-red-800 text-white' : 
                  'bg-blue-800 text-white']">
          <span>{{ toast.message }}</span>
          <button @click="() => removeToast(toast.id)" class="ml-2 text-white">&times;</button>
        </div>
      </div>

      <div class="flex justify-between items-center container mx-auto px-4">
        <!-- Logo -->
        <router-link to="/" class="inline-block">
          <img src="@/assets/pustakalaya-logo.png" alt="Pustakalaya Logo"
               class="w-12 h-12 md:w-14 md:h-14"/>
        </router-link>

        <!-- Center Links -->
        <ul class="hidden md:flex space-x-6">
          <li>
            <router-link to="/" class="text-lg font-semibold hover:text-[#A47148]"
                         exact-active-class="text-white font-bold">Home
            </router-link>
          </li>
          <li>
            <router-link to="/shop" class="text-lg font-semibold hover:text-[#A47148]"
                         exact-active-class="text-white font-bold">Shop
            </router-link>
          </li>
          <li v-if="!isLoggedIn">
            <router-link to="/auth" class="text-lg font-semibold hover:text-[#A47148]"
                         exact-active-class="text-white font-bold">Login
            </router-link>
          </li>
        </ul>

        <!-- Right Icons -->
        <div class="flex items-center space-x-6 relative">
          <!-- Wishlist Icon -->
          <router-link to="/wishlist" class="relative hover:text-[#FFD700]">
            <font-awesome-icon :icon="['fas','heart']" class="text-2xl"/>
            <span v-if="wishlistItems.length > 0"
                  class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
              {{ wishlistItems.length }}
            </span>
          </router-link>

          <!-- Cart Icon -->
          <router-link to="/cart" class="relative hover:text-[#FFD700]">
            <font-awesome-icon :icon="['fas','shopping-cart']" class="text-2xl"/>
            <span v-if="cartItemCount > 0"
                  class="absolute -top-1 -right-1 bg-red-500 text-white text-xs rounded-full w-5 h-5 flex items-center justify-center">
              {{ cartItemCount }}
            </span>
          </router-link>

          <!-- Profile Dropdown -->
          <div v-if="isLoggedIn" class="relative">
            <button @click="toggleProfileDropdown" class="hover:text-[#FFD700]">
              <font-awesome-icon :icon="['fas','user']" class="text-2xl"/>
            </button>
            <div v-if="showProfileDropdown"
                 class="absolute right-0 mt-2 w-40 bg-[#121212] text-[#C8B280] shadow-lg rounded-lg py-2 z-50">
              <router-link to="/profile"
                           class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Profile
              </router-link>
              <button @click="logout"
                      class="block w-full text-left px-4 py-2 text-sm hover:bg-[#1c1c1c]">
                Logout
              </button>
            </div>
          </div>

          <!-- Mobile Menu -->
          <div class="md:hidden relative">
            <button @click="isMobileOpen = !isMobileOpen" class="text-[#C8B280]">
              <svg class="w-6 h-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2"
                      d="M4 6h16M4 12h16M4 18h16"/>
              </svg>
            </button>
            <ul v-if="isMobileOpen"
                class="absolute right-0 mt-2 w-48 bg-[#121212] text-[#C8B280] shadow-lg rounded-lg py-2 z-50">
              <li>
                <router-link to="/" class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Home</router-link>
              </li>
              <li>
                <router-link to="/shop" class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Shop</router-link>
              </li>
              <li>
                <router-link to="/wishlist" class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Wishlist</router-link>
              </li>
              <li>
                <router-link to="/cart" class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Cart</router-link>
              </li>
              <li v-if="!isLoggedIn">
                <router-link to="/auth" class="block px-4 py-2 text-sm hover:bg-[#1c1c1c]">Login</router-link>
              </li>
              <li v-else>
                <button @click="logout" class="block w-full text-left px-4 py-2 text-sm hover:bg-[#1c1c1c]">Logout
                </button>
              </li>
            </ul>
          </div>
        </div>
      </div>
    </nav>

    <!-- Main Content -->
    <main class="flex-1 container mx-auto px-4 py-6">
      <h1 class="text-3xl font-bold text-[#F5DEB3] mb-6">Your Wishlist</h1>

      <!-- Empty state -->
      <div v-if="!wishlistItems.length" class="flex flex-col items-center justify-center text-[#aaa] py-20">
        <p class="text-lg mb-4">Your wishlist is currently empty.</p>
        <router-link
            to="/shop"
            class="px-6 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition"
        >Browse Books
        </router-link>
      </div>

      <!-- Wishlist items -->
      <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
        <transition-group name="fade" tag="div" class="contents">
          <div
              v-for="item in wishlistItems"
              :key="item.id"
              class="bg-[#1a1a1a] rounded-xl p-5 flex flex-col shadow border border-[#333] hover:shadow-md transition"
          >
            <div class="relative">
              <img
                  :src="item.images[0] || '/fallback.png'"
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
                  @click="addToCart(item)"
                  class="px-4 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition flex-1 mr-2"
              >
                <i class="fas fa-shopping-cart mr-2"></i>Add to Cart
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

      <!-- Action buttons -->
      <div v-if="wishlistItems.length"
           class="mt-8 flex flex-col md:flex-row justify-center space-y-4 md:space-y-0 md:space-x-4">
        <button
            @click="addAllToCart"
            class="px-6 py-2 bg-[#A0522D] text-white rounded hover:bg-[#8B4513] transition"
        >Add All to Cart
        </button>
        <button
            @click="clearWishlist"
            class="px-6 py-2 bg-[#8B0000] text-white rounded hover:bg-[#a30000] transition"
        >Clear Wishlist
        </button>
      </div>
    </main>

    <!-- Share Modal -->
    <div v-if="showShareModal" class="fixed inset-0 bg-black bg-opacity-75 flex items-center justify-center z-50">
      <div class="bg-[#1a1a1a] p-6 rounded-xl w-full max-w-md">
        <h3 class="text-xl font-bold text-[#F5DEB3] mb-4">Share This Book</h3>

        <div class="flex flex-col space-y-4">
          <div class="flex items-center space-x-2">
            <input
                type="text"
                :value="shareUrl"
                readonly
                class="flex-1 bg-[#333] border border-[#444] text-[#C8B280] px-3 py-2 rounded"
            />
            <button
                @click="copyToClipboard"
                class="px-3 py-2 bg-[#444] rounded hover:bg-[#555]"
            >
              <i class="fas fa-copy"></i>
            </button>
          </div>

          <div class="flex justify-center space-x-6 mt-2">
            <a href="#" @click.prevent="shareOnSocial('facebook')" class="text-2xl text-[#3b5998] hover:opacity-80">
              <i class="fab fa-facebook"></i>
            </a>
            <a href="#" @click.prevent="shareOnSocial('twitter')" class="text-2xl text-[#1da1f2] hover:opacity-80">
              <i class="fab fa-twitter"></i>
            </a>
            <a href="#" @click.prevent="shareOnSocial('whatsapp')" class="text-2xl text-[#25d366] hover:opacity-80">
              <i class="fab fa-whatsapp"></i>
            </a>
          </div>
        </div>

        <button
            @click="showShareModal = false"
            class="mt-6 w-full px-4 py-2 bg-[#333] text-[#C8B280] rounded hover:bg-[#444] transition"
        >Close
        </button>
      </div>
    </div>

    <!-- Footer Component -->
    <footer class="bg-[#23110d] py-8 text-[#C8B280]">
      <div class="container mx-auto text-center">
        <!-- Branding -->
        <p>&copy; 2024 Pustakalaya — All Rights Reserved</p>

        <!-- Social Links -->
        <div class="mt-4">
          <p class="text-[#C8B280]">Follow us on:</p>
          <div class="flex justify-center space-x-6 mt-2 text-sm">
            <a href="#" aria-label="Facebook" class="flex items-center hover:text-[#FFD700] transition duration-300">
              <i class="fab fa-facebook-f mr-2"></i> Facebook
            </a>
            <a href="#" aria-label="Instagram" class="flex items-center hover:text-[#FFD700] transition duration-300">
              <i class="fab fa-instagram mr-2"></i> Instagram
            </a>
            <a href="#" aria-label="Twitter" class="flex items-center hover:text-[#FFD700] transition duration-300">
              <i class="fab fa-twitter mr-2"></i> Twitter
            </a>
          </div>
        </div>

        <!-- Additional Links -->
        <div class="mt-6 space-x-2 text-sm">
          <a href="/privacy-policy" class="hover:text-[#FFD700] transition duration-300">Privacy Policy</a>
          <span class="text-gray-600">|</span>
          <a href="/terms" class="hover:text-[#FFD700] transition duration-300">Terms of Service</a>
          <span class="text-gray-600">|</span>
          <a href="/contact" class="hover:text-[#FFD700] transition duration-300">Contact Us</a>
        </div>
      </div>
    </footer>
  </div>
</template>

<script setup>
import {ref, computed, reactive, onMounted} from 'vue'
import {useRouter} from 'vue-router'
import axios from 'axios'

// Base URL from webpack env
const baseUrl = process.env.VUE_APP_API_URL || window.location.origin
const router = useRouter()

// --- Auth & UI State ---
const isLoggedIn = ref(false)
const showProfileDropdown = ref(false)
const isMobileOpen = ref(false)
const showShareModal = ref(false)
const currentShareItem = ref(null)
const shareUrl = ref('')
const cartItemCount = ref(0)

// --- Store Setup: Wishlist & Cart ---
// 1. Wishlist State
const wishlistItems = ref([])

// 2. Cart State & Functions
// Actual cart functions that would interact with your API
async function addToCart(item) {
  try {
    const response = await axios.post(`${baseUrl}/api/v1/cart`, {
      productId: item.id,
      quantity: 1
    }, {
      headers: authHeader()
    })

    showToast('Item added to cart')
    fetchCartCount() // Refresh cart count
    return {success: true, message: 'Item added to cart'}
  } catch (err) {
    console.error('Failed to add item to cart', err)
    showToast('Failed to add item to cart', 'error')
    return {success: false, message: err.response?.data?.message || 'Failed to add item to cart'}
  }
}

// --- Toast Notification System ---
const toasts = reactive([])
const removeToast = id => {
  const idx = toasts.findIndex(t => t.id === id)
  if (idx > -1) toasts.splice(idx, 1)
}
const showToast = (msg, type = 'success') => {
  const id = Date.now() + Math.random()
  toasts.push({id, message: msg, type})
  setTimeout(() => removeToast(id), 5000)
}

// --- Auth Helper Function ---
const authHeader = () => {
  const token = localStorage.getItem('authToken')
  return token ? {Authorization: `Bearer ${token}`} : {}
}

// --- Wishlist Functions ---
// 1. Fetch wishlist
async function fetchWishlist() {
  try {
    const response = await axios.get(`${baseUrl}/api/v1/wishlist`, {
      headers: authHeader()
    })

    // Process and normalize the response data
    const rawItems = response.data.data || []
    wishlistItems.value = processWishlistItems(rawItems)
  } catch (err) {
    console.error('Failed to fetch wishlist', err)
    showToast('Failed to load wishlist items', 'error')
  }
}

// 2. Process wishlist items
function processWishlistItems(items) {
  return items.map(i => {
    let id, title, description, price, rawImgs

    if ('bookId' in i) {
      id = i.bookId;
      title = i.title;
      description = i.description;
      price = i.price;
      rawImgs = i.images
    } else if (i.product?.bookId !== undefined) {
      id = i.product.bookId;
      title = i.product.title || i.product.name;
      description = i.product.description;
      price = i.product.price;
      rawImgs = i.product.images
    } else if (i.product?.id !== undefined) {
      id = i.product.id;
      title = i.product.name;
      description = i.product.description;
      price = i.product.price;
      rawImgs = i.product.images
    } else {
      console.error('Unknown item shape', i)
      return null
    }

    const images = (rawImgs || []).map(p =>
        p.startsWith('http') ? p : `${baseUrl.replace(/\/$/, '')}${p.startsWith('/') ? '' : '/'}${p}`
    )

    return {id, title, description, price, images}
  }).filter(Boolean)
}

// 3. Remove from wishlist
async function removeFromWishlist(item) {
  try {
    await axios.delete(`${baseUrl}/api/v1/wishlist/${item.id}`, {
      headers: authHeader()
    })

    // Remove item locally
    wishlistItems.value = wishlistItems.value.filter(i => i.id !== item.id)
    showToast('Item removed from wishlist')
    return {success: true, message: 'Item removed from wishlist'}
  } catch (err) {
    console.error('Failed to remove item from wishlist', err)
    showToast('Failed to remove item from wishlist', 'error')
    return {success: false, message: err.response?.data?.message || 'Failed to remove item'}
  }
}

// 4. Clear wishlist
async function clearWishlist() {
  try {
    await axios.delete(`${baseUrl}/api/v1/wishlist`, {
      headers: authHeader()
    })

    // Clear local wishlist
    wishlistItems.value = []
    showToast('Wishlist cleared')
    return {success: true, message: 'Wishlist cleared'}
  } catch (err) {
    console.error('Failed to clear wishlist', err)
    showToast('Failed to clear wishlist', 'error')
    return {success: false, message: err.response?.data?.message || 'Failed to clear wishlist'}
  }
}

// 5. Add all to cart
async function addAllToCart() {
  try {
    const promises = wishlistItems.value.map(item => addToCart(item))
    await Promise.all(promises)
    showToast('All items added to cart')
  } catch (err) {
    console.error('Failed to add all items to cart', err)
    showToast('Failed to add all items to cart', 'error')
  }
}

// --- Share Functionality ---
function shareItem(item) {
  currentShareItem.value = item
  shareUrl.value = `${window.location.origin}/shop/book/${item.id}`
  showShareModal.value = true
}

function copyToClipboard() {
  navigator.clipboard.writeText(shareUrl.value)
      .then(() => showToast('Link copied to clipboard'))
      .catch(() => showToast('Failed to copy link', 'error'))
}

function shareOnSocial(platform) {
  const item = currentShareItem.value
  const text = `Check out "${item.title}" on Pustakalaya!`
  const url = shareUrl.value

  let shareUrl
  switch (platform) {
    case 'facebook':
      shareUrl = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`
      break
    case 'twitter':
      shareUrl = `https://twitter.com/intent/tweet?text=${encodeURIComponent(text)}&url=${encodeURIComponent(url)}`
      break
    case 'whatsapp':
      shareUrl = `https://wa.me/?text=${encodeURIComponent(text + ' ' + url)}`
      break
  }

  window.open(shareUrl, '_blank')
  showShareModal.value = false
}

// --- Profile Dropdown Toggle ---
function toggleProfileDropdown() {
  showProfileDropdown.value = !showProfileDropdown.value
}

// --- Logout Function ---
async function logout() {
  try {
    await axios.post(`${baseUrl}/api/v1/logout`, {}, {
      headers: authHeader()
    })
  } catch {
  }
  localStorage.clear()
  isLoggedIn.value = false
  router.push('/auth')
}

// --- Cart Count ---
async function fetchCartCount() {
  try {
    const response = await axios.get(`${baseUrl}/api/v1/cart`, {
      headers: authHeader()
    })

    // Extract cart count from response
    const cartData = response.data.data || {}
    const items = cartData.cart?.products || []
    cartItemCount.value = items.reduce((sum, item) => sum + item.quantity, 0)
  } catch (err) {
    console.error('Failed to fetch cart count', err)
    cartItemCount.value = 0
  }
}

// --- Initialization ---
onMounted(() => {
  isLoggedIn.value = !!localStorage.getItem('authToken')
  if (isLoggedIn.value) {
    fetchWishlist()
    fetchCartCount()
  }
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

.fade-enter-from {
  opacity: 0;
  transform: translateY(10px);
}

.fade-enter-to {
  opacity: 1;
  transform: translateY(0);
}

.fade-leave-to {
  opacity: 0;
  transform: translateY(10px);
}

.contents {
  display: contents;
}
</style>